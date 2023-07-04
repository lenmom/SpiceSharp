using System;

using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="IBiasingBehavior"/> for a <see cref="Parallel"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBiasingBehavior" />
    public partial class Biasing : Behavior,
        IParallelBehavior,
        IBiasingBehavior
    {
        private readonly BiasingSimulationState _state;
        private readonly Workload _loadWorkload;
        private BehaviorList<IBiasingBehavior> _biasingBehaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Biasing(ParallelBindingContext context)
            : base(context)
        {
            Parameters parameters = context.GetParameterSet<Parameters>();
            if (parameters.WorkDistributors.TryGetValue(typeof(IBiasingBehavior), out IWorkDistributor dist) && dist != null)
            {
                _loadWorkload = new Workload(dist, parameters.Entities.Count);
                if (context.TryGetState(out IBiasingSimulationState bparent))
                {
                    context.AddLocalState<IBiasingSimulationState>(_state = new BiasingSimulationState(bparent));
                }

                if (context.TryGetState(out IIterationSimulationState cparent))
                {
                    context.AddLocalState<IIterationSimulationState>(new IterationSimulationState(cparent));
                }
            }
        }

        /// <inheritdoc/>
        public virtual void FetchBehaviors(ParallelBindingContext context)
        {
            _biasingBehaviors = context.GetBehaviors<IBiasingBehavior>();
            if (_loadWorkload != null)
            {
                foreach (IBiasingBehavior behavior in _biasingBehaviors)
                {
                    _loadWorkload.Actions.Add(behavior.Load);
                }
            }
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            if (_loadWorkload != null)
            {
                _state.Reset();
                _loadWorkload.Execute();
                _state.Apply();
            }
            else
            {
                foreach (IBiasingBehavior behavior in _biasingBehaviors)
                {
                    behavior.Load();
                }
            }
        }
    }
}
