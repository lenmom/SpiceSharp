﻿using System;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Base class with common methods
    /// </summary>
    public abstract class BaseSimulation : Simulation
    {
        /// <summary>
        /// Gets the currently active configuration
        /// </summary>
        public BaseConfiguration BaseConfiguration { get; protected set; }

        /// <summary>
        /// Gets the currently active state
        /// </summary>
        public RealState RealState { get; protected set; }

        /// <summary>
        /// Gets statistics
        /// </summary>
        public Statistics Statistics { get; } = new Statistics();

        /// <summary>
        /// The node that gives problems
        /// </summary>
        public Variable ProblemVariable { get; protected set; }

        #region Events
        /// <summary>
        /// Event called before the current state is loaded
        /// </summary>
        public event EventHandler<LoadStateEventArgs> BeforeLoad;

        /// <summary>
        /// Event called after the current state is loaded
        /// </summary>
        public event EventHandler<LoadStateEventArgs> AfterLoad; 

        /// <summary>
        /// Event called before temperature calculations
        /// </summary>
        public event EventHandler<EventArgs> BeforeTemperature;

        /// <summary>
        /// Event called after temperature calculations
        /// </summary>
        public event EventHandler<EventArgs> AfterTemperature;
        #endregion

        /// <summary>
        /// Private variables
        /// </summary>
        private LoadStateEventArgs _realStateLoadArgs;
        private BehaviorList<BaseLoadBehavior> _loadBehaviors;
        private BehaviorList<BaseTemperatureBehavior> _temperatureBehaviors;
        private BehaviorList<BaseInitialConditionBehavior> _initialConditionBehaviors;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        protected BaseSimulation(Identifier name)
            : base(name)
        {
            ParameterSets.Add(new BaseConfiguration());
        }

        /// <summary>
        /// Setup the simulation
        /// </summary>
        /// <param name="circuit">Circuit</param>
        protected override void Setup(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));
            base.Setup(circuit);

            // Setup behaviors, configurations and states
            BaseConfiguration = ParameterSets.Get<BaseConfiguration>();
            _temperatureBehaviors = SetupBehaviors<BaseTemperatureBehavior>(circuit.Objects);
            _loadBehaviors = SetupBehaviors<BaseLoadBehavior>(circuit.Objects);
            _initialConditionBehaviors = SetupBehaviors<BaseInitialConditionBehavior>(circuit.Objects);

            // Add a state for real numbers
            RealState = new RealState();
            States.Add(RealState);

            // Setup the load behaviors
            _realStateLoadArgs = new LoadStateEventArgs(RealState);
            for (var i = 0; i < _loadBehaviors.Count; i++)
                _loadBehaviors[i].GetEquationPointers(Nodes, RealState.Solver);
            RealState.Initialize(Nodes);

            // Allow nodesets to help convergence
            AfterLoad += LoadNodeSets;
        }

        /// <summary>
        /// Execute the simulation
        /// </summary>
        protected override void Execute()
        {
            Temperature();

            // Do initial conditions
            InitialConditions();
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        protected void Temperature()
        {
            OnBeforeTemperature(EventArgs.Empty);
            for (var i = 0; i < _temperatureBehaviors.Count; i++)
                _temperatureBehaviors[i].Temperature(this);
            OnAfterTemperature(EventArgs.Empty);
        }

        /// <summary>
        /// Unsetup the simulation
        /// </summary>
        protected override void Unsetup()
        {
            base.Unsetup();

            // Remove nodeset
            AfterLoad -= LoadNodeSets;

            // Unsetup all behaviors
            for (var i = 0; i < _initialConditionBehaviors.Count; i++)
                _initialConditionBehaviors[i].Unsetup();
            for (var i = 0; i < _loadBehaviors.Count; i++)
                _loadBehaviors[i].Unsetup();
            for (var i = 0; i < _temperatureBehaviors.Count; i++)
                _temperatureBehaviors[i].Unsetup();

            // Clear the state
            RealState.Destroy();
            RealState = null;
            _realStateLoadArgs = null;

            // Remove behavior and configuration references
            _loadBehaviors = null;
            _initialConditionBehaviors = null;
            _temperatureBehaviors = null;
            BaseConfiguration = null;
        }

        /// <summary>
        /// Calculate the operating point of the circuit
        /// </summary>
        /// <param name="maxIterations">Maximum iterations</param>
        protected void Op(int maxIterations)
        {
            var state = RealState;
            var config = BaseConfiguration;
            state.Init = RealState.InitializationStates.InitJunction;

            // First, let's try finding an operating point by using normal iterations
            if (!config.NoOperatingPointIterations)
            {
                if (Iterate(maxIterations))
                {
                    return;
                }
            }

            // No convergence, try Gmin stepping
            if (config.GminSteps > 1)
            {
                state.Init = RealState.InitializationStates.InitJunction;
                CircuitWarning.Warning(this, Properties.Resources.StartGminStepping);
                state.DiagonalGmin = config.Gmin;
                for (var i = 0; i < config.GminSteps; i++)
                    state.DiagonalGmin *= 10.0;
                for (var i = 0; i <= config.GminSteps; i++)
                {
                    state.IsConvergent = false;
                    if (!Iterate(maxIterations))
                    {
                        state.DiagonalGmin = 0.0;
                        CircuitWarning.Warning(this, Properties.Resources.GminSteppingFailed);
                        break;
                    }
                    state.DiagonalGmin /= 10.0;
                    state.Init = RealState.InitializationStates.InitFloat;
                }
                state.DiagonalGmin = 0.0;
                if (Iterate(maxIterations))
                {
                    return;
                }
            }

            // Nope, still not converging, let's try source stepping
            if (config.SourceSteps > 1)
            {
                state.Init = RealState.InitializationStates.InitJunction;
                CircuitWarning.Warning(this, Properties.Resources.StartSourceStepping);
                for (var i = 0; i <= config.SourceSteps; i++)
                {
                    state.SourceFactor = i / (double)config.SourceSteps;
                    if (!Iterate(maxIterations))
                    {
                        state.SourceFactor = 1.0;
                        CircuitWarning.Warning(this, Properties.Resources.SourceSteppingFailed);
                        return;
                    }
                }
                state.SourceFactor = 1.0;
                return;
            }

            // Failed
            throw new CircuitException("Could not determine operating point");
        }

        /// <summary>
        /// Solve iteratively for simulations
        /// </summary>
        /// <param name="maxIterations">Maximum number of iterations</param>
        /// <returns></returns>
        protected bool Iterate(int maxIterations)
        {
            var state = RealState;
            var solver = state.Solver;
            var pass = false;
            var iterno = 0;

            // Ignore operating condition point, just use the solution as-is
            if (state.UseIc && state.Domain == RealState.DomainType.Time)
            {
                state.StoreSolution();

                // Voltages are set using IC statement on the nodes
                // Internal initial conditions are calculated by the components
                Load();
                return true;
            }

            // Perform iteration
            while (true)
            {
                // Reset convergence flag
                state.IsConvergent = true;

                try
                {
                    Load();
                    iterno++;
                }
                catch (CircuitException)
                {
                    iterno++;
                    Statistics.Iterations = iterno;
                    throw;
                }

                // Preorder matrix
                if ((state.Sparse & RealState.SparseStates.DidPreorder) == 0)
                {
                    solver.PreorderModifiedNodalAnalysis(Math.Abs);
                    state.Sparse |= RealState.SparseStates.DidPreorder;
                }
                if (state.Init == RealState.InitializationStates.InitJunction || state.Init == RealState.InitializationStates.InitTransient)
                {
                    state.Sparse |= RealState.SparseStates.ShouldReorder;
                }

                // Reorder
                if ((state.Sparse & RealState.SparseStates.ShouldReorder) != 0) // state.Sparse.HasFlag(RealState.SparseStates.ShouldReorder)
                {
                    Statistics.ReorderTime.Start();
                    solver.ApplyDiagonalGmin(state.DiagonalGmin);
                    solver.OrderAndFactor();
                    Statistics.ReorderTime.Stop();
                    state.Sparse &= ~RealState.SparseStates.ShouldReorder;
                }
                else
                {
                    // Decompose
                    Statistics.DecompositionTime.Start();
                    solver.ApplyDiagonalGmin(state.DiagonalGmin);
                    var success = solver.Factor();
                    Statistics.DecompositionTime.Stop();

                    if (!success)
                    {
                        state.Sparse |= RealState.SparseStates.ShouldReorder;
                        continue;
                    }
                }

                // The current solution becomes the old solution
                state.StoreSolution();

                // Solve the equation
                Statistics.SolveTime.Start();
                solver.Solve(state.Solution);
                Statistics.SolveTime.Stop();

                // Reset ground nodes
                solver.GetRhsElement(0).Value = 0.0;
                state.Solution[0] = 0.0;
                state.OldSolution[0] = 0.0;

                // Exceeded maximum number of iterations
                if (iterno > maxIterations)
                {
                    Statistics.Iterations += iterno;
                    return false;
                }

                if (state.IsConvergent && iterno != 1)
                    state.IsConvergent = IsConvergent();
                else
                    state.IsConvergent = false;

                switch (state.Init)
                {
                    case RealState.InitializationStates.InitFloat:
                        if (state.UseDc && state.HadNodeSet)
                        {
                            if (pass)
                                state.IsConvergent = false;
                            pass = false;
                        }
                        if (state.IsConvergent)
                        {
                            Statistics.Iterations += iterno;
                            return true;
                        }
                        break;

                    case RealState.InitializationStates.InitJunction:
                        state.Init = RealState.InitializationStates.InitFix;
                        state.Sparse |= RealState.SparseStates.ShouldReorder;
                        break;

                    case RealState.InitializationStates.InitFix:
                        if (state.IsConvergent)
                            state.Init = RealState.InitializationStates.InitFloat;
                        pass = true;
                        break;

                    case RealState.InitializationStates.InitTransient:
                        if (iterno <= 1)
                            state.Sparse = RealState.SparseStates.ShouldReorder;
                        state.Init = RealState.InitializationStates.InitFloat;
                        break;

                    case RealState.InitializationStates.None:
                        state.Init = RealState.InitializationStates.InitFloat;
                        break;

                    default:
                        Statistics.Iterations += iterno;
                        throw new CircuitException("Could not find flag");
                }
            }
        }

        /// <summary>
        /// Load the circuit with the load behaviors
        /// </summary>
        protected void Load()
        {
            var state = RealState;

            // Start the stopwatch
            Statistics.LoadTime.Start();
            OnBeforeLoad(_realStateLoadArgs);

            // Clear rhs and matrix
            state.Solver.Clear();
            LoadBehaviors();

            // Keep statistics
            OnAfterLoad(_realStateLoadArgs);
            Statistics.LoadTime.Stop();
        }

        /// <summary>
        /// Load all behaviors
        /// </summary>
        protected virtual void LoadBehaviors()
        {
            for (var i = 0; i < _loadBehaviors.Count; i++)
                _loadBehaviors[i].Load(this);
        }

        /// <summary>
        /// Set the initial conditions
        /// </summary>
        protected void InitialConditions()
        {
            var state = RealState;
            var nodes = Nodes;
            var solver = state.Solver;

            // Clear the current solution
            var element = solver.FirstInReorderedRhs();
            while (element != null)
            {
                element.Value = 0.0;
                element = element.Below;
            }

            // Go over all nodes
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (nodes.NodeSets.ContainsKey(node.Name))
                {
                    node.Diagonal = solver.GetMatrixElement(node.Index, node.Index);

                    // Avoid creating a sparse element if it is not needed
                    if (!nodes.NodeSets[node.Name].Equals(0.0))
                        solver.GetRhsElement(node.Index).Value = nodes.NodeSets[node.Name];
                    state.HadNodeSet = true;
                }
                if (nodes.InitialConditions.ContainsKey(node.Name))
                {
                    node.Diagonal = solver.GetMatrixElement(node.Index, node.Index);

                    // Avoid creating a sparse element if it is not needed
                    if (!nodes.InitialConditions[node.Name].Equals(0.0))
                        solver.GetRhsElement(node.Index).Value = nodes.InitialConditions[node.Name];
                }
            }

            // Use initial conditions
            if (state.UseIc)
            {
                for (var i = 0; i < _initialConditionBehaviors.Count; i++)
                    _initialConditionBehaviors[i].SetInitialCondition(this);
            }
        }

        /// <summary>
        /// Apply nodesets
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected void LoadNodeSets(object sender, LoadStateEventArgs e)
        {
            var state = RealState;
            var nodes = Nodes;

            // Consider doing nodeset & ic assignments
            if ((state.Init & (RealState.InitializationStates.InitJunction | RealState.InitializationStates.InitFix)) != 0)
            {
                // Do nodesets
                for (var i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    if (nodes.NodeSets.ContainsKey(node.Name))
                    {
                        var ns = nodes.NodeSets[node.Name];
                        if (ZeroNoncurrentRow(state.Solver, nodes, node.Index))
                        {
                            if (!ns.Equals(0.0))
                                state.Solver.GetRhsElement(node.Index).Value = 1.0e10 * ns;
                            node.Diagonal.Value = 1.0e10;
                        }
                        else
                        {
                            if (!ns.Equals(0.0))
                                state.Solver.GetRhsElement(node.Index).Value = ns;
                            node.Diagonal.Value = 1.0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reset the row to 0.0 and return true if the row is a current equation
        /// </summary>
        /// <param name="solver">Solver</param>
        /// <param name="variables">List of unknowns/variables</param>
        /// <param name="rowIndex">Row number</param>
        /// <returns></returns>
        protected static bool ZeroNoncurrentRow(SparseLinearSystem<double> solver, VariableSet variables, int rowIndex)
        {
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));
            if (variables == null)
                throw new ArgumentNullException(nameof(variables));

            var currents = false;
            for (var n = 0; n < variables.Count; n++)
            {
                var node = variables[n];
                var x = solver.FindMatrixElement(rowIndex, node.Index);
                if (x != null && !x.Value.Equals(0.0))
                {
                    if (node.UnknownType == VariableType.Current)
                        currents = true;
                    else
                        x.Value = 0.0;
                }
            }
            return currents;
        }

        /// <summary>
        /// Check if we are converging during iterations
        /// </summary>
        /// <returns></returns>
        protected bool IsConvergent()
        {
            var rstate = RealState;
            var config = BaseConfiguration;

            // Check convergence for each node
            for (var i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                var n = rstate.Solution[node.Index];
                var o = rstate.OldSolution[node.Index];

                if (double.IsNaN(n))
                    throw new CircuitException("Non-convergence, node {0} is not a number.".FormatString(node));

                if (node.UnknownType == VariableType.Voltage)
                {
                    var tol = config.RelativeTolerance * Math.Max(Math.Abs(n), Math.Abs(o)) + config.VoltageTolerance;
                    if (Math.Abs(n - o) > tol)
                    {
                        ProblemVariable = node;
                        return false;
                    }
                }
                else
                {
                    var tol = config.RelativeTolerance * Math.Max(Math.Abs(n), Math.Abs(o)) + config.AbsoluteTolerance;
                    if (Math.Abs(n - o) > tol)
                    {
                        ProblemVariable = node;
                        return false;
                    }
                }
            }

            // Device-level convergence tests
            for (var i = 0; i < _loadBehaviors.Count; i++)
            {
                if (!_loadBehaviors[i].IsConvergent(this))
                {
                    // I believe this should be false, but Spice 3f5 doesn't...

                    /*
                     * Each device that checks convergence returns (OK) = 0 regardless
                     * of convergence (eg. Dev2/mos2conv.c). Not being convergent is communicated
                     * through the CKTnoncon variable (state.IsCon for Spice#).
                     * 
                     * The convergence methods are called in CKT/cktop.c at line 121. If an error
                     * occurs, it is returned. If non-convergence is detected through CKTnoncon,
                     * (OK) is returned anyway, so it doesn't make a difference. Remember that 
                     * our devices aren't returning anything else but (OK) anyway.
                     * 
                     * CKTconvTest in turn is called in NI/niconv.c at line 65. The result is
                     * therefore always (OK) too, and so the returned value by NIconvTest()
                     * is always (OK) if each device has tested convergence. Note that 1 is
                     * returned in the case of non-convergence for nodes!
                     * 
                     * Finally, in NI/niiter.c at line 184, when convergence is tested, the result
                     * is used to overwrite CKTnoncon, so there is no way we can still find out if
                     * any device detected non-convergence.
                     */
                    return true;
                }
            }

            // Convergence succeeded
            return true;
        }

        #region Methods for calling events
        /// <summary>
        /// Call event just before loading
        /// </summary>
        protected virtual void OnBeforeLoad(LoadStateEventArgs args) => BeforeLoad?.Invoke(this, args);

        /// <summary>
        /// Call event just after loading
        /// </summary>
        protected virtual void OnAfterLoad(LoadStateEventArgs args) => AfterLoad?.Invoke(this, args);

        /// <summary>
        /// Call event just before temperature calculations
        /// </summary>
        protected virtual void OnBeforeTemperature(EventArgs args) => BeforeTemperature?.Invoke(this, args);

        /// <summary>
        /// Call event just after temperature calculations
        /// </summary>
        protected virtual void OnAfterTemperature(EventArgs args) => AfterTemperature?.Invoke(this, args);

        #endregion
    }
}
