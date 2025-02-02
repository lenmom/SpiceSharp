﻿using System;

using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An <see cref="IConvergenceBehavior"/> for a <see cref="Parallel"/>.
    /// </summary>
    /// <seealso cref="Biasing" />
    /// <seealso cref="IConvergenceBehavior" />
    [BehaviorFor(typeof(Parallel))]
    public partial class Convergence : Biasing,
        IParallelBehavior,
        IConvergenceBehavior
    {
        private BehaviorList<IConvergenceBehavior> _convergenceBehaviors;
        private readonly Workload<bool> _convergenceWorkload;

        /// <summary>
        /// Initializes a new instance of the <see cref="Convergence" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Convergence(ParallelBindingContext context)
            : base(context)
        {
            Parameters parameters = context.GetParameterSet<Parameters>();
            if (parameters.WorkDistributors.TryGetValue(typeof(IConvergenceBehavior), out IWorkDistributor dist) && dist != null)
            {
                _convergenceWorkload = new Workload<bool>((IWorkDistributor<bool>)dist, parameters.Entities.Count);
                if (context.TryGetState<IIterationSimulationState>(out IIterationSimulationState parent))
                {
                    if (!(parent is IterationSimulationState))
                    {
                        context.AddLocalState<IIterationSimulationState>(new IterationSimulationState(parent));
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void FetchBehaviors(ParallelBindingContext context)
        {
            base.FetchBehaviors(context);
            _convergenceBehaviors = context.GetBehaviors<IConvergenceBehavior>();
            if (_convergenceWorkload != null)
            {
                foreach (IConvergenceBehavior behavior in _convergenceBehaviors)
                {
                    _convergenceWorkload.Functions.Add(behavior.IsConvergent);
                }
            }
        }

        /// <inheritdoc/>
        bool IConvergenceBehavior.IsConvergent()
        {
            if (_convergenceWorkload != null)
            {
                return _convergenceWorkload.Execute();
            }
            else
            {
                bool convergence = true;
                foreach (IConvergenceBehavior behavior in _convergenceBehaviors)
                {
                    convergence &= behavior.IsConvergent();
                }

                return convergence;
            }
        }
    }
}
