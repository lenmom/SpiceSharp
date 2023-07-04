using System;

using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// A binding context for a <see cref="VoltageSwitch"/>.
    /// </summary>
    /// <seealso cref="ComponentBindingContext" />
    /// <seealso cref="ISwitchBindingContext" />
    [BindingContextFor(typeof(VoltageSwitch))]
    public class VoltageSwitchBindingContext : ComponentBindingContext,
        ISwitchBindingContext
    {
        /// <inheritdoc/>
        public Func<double> ControlValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSwitchBindingContext"/> class.
        /// </summary>
        /// <param name="component">The component creating the behavior.</param>
        /// <param name="simulation">The simulation for which a behavior is created.</param>
        /// <param name="behaviors">The behaviors created by the entity.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="simulation"/>, <paramref name="component"/> or <paramref name="behaviors"/> is <c>null</c>.</exception>
        public VoltageSwitchBindingContext(IComponent component, ISimulation simulation, IBehaviorContainer behaviors)
            : base(component, simulation, behaviors)
        {
            IBiasingSimulationState state = GetState<IBiasingSimulationState>();
            IVariable<double> a = state.GetSharedVariable(Nodes[2]);
            IVariable<double> b = state.GetSharedVariable(Nodes[3]);
            ControlValue = () => a.Value - b.Value;
        }
    }
}
