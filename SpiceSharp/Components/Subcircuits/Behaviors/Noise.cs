using System;
using System.Linq;

using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// An <see cref="INoiseBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="INoiseBehavior" />
    [BehaviorFor(typeof(Subcircuit))]
    public class Noise : SubcircuitBehavior<INoiseBehavior>,
        INoiseBehavior
    {
        /// <inheritdoc/>
        public double OutputNoiseDensity
        {
            get
            {
                return Behaviors.Sum(nb => nb.OutputNoiseDensity);
            }
        }

        /// <inheritdoc/>
        public double TotalOutputNoise
        {
            get
            {
                return Behaviors.Sum(nb => nb.TotalOutputNoise);
            }
        }

        /// <inheritdoc/>
        public double TotalInputNoise
        {
            get
            {
                return Behaviors.Sum(nb => nb.TotalInputNoise);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Noise(SubcircuitBindingContext context)
            : base(context)
        {
        }

        /// <inheritdoc/>
        void INoiseSource.Initialize()
        {
            foreach (INoiseBehavior behavior in Behaviors)
            {
                behavior.Initialize();
            }
        }

        /// <inheritdoc/>
        void INoiseBehavior.Compute()
        {
            foreach (INoiseBehavior behavior in Behaviors)
            {
                behavior.Compute();
            }
        }
    }
}
