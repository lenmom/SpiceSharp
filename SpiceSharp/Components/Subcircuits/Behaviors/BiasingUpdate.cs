using System;

using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// An <see cref="IBiasingUpdateBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IBiasingUpdateBehavior" />
    [BehaviorFor(typeof(Subcircuit))]
    public class BiasingUpdate : SubcircuitBehavior<IBiasingUpdateBehavior>,
        IBiasingUpdateBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingUpdate" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public BiasingUpdate(SubcircuitBindingContext context)
            : base(context)
        {
        }

        /// <inheritdoc/>
        void IBiasingUpdateBehavior.Update()
        {
            foreach (IBiasingUpdateBehavior behavior in Behaviors)
            {
                behavior.Update();
            }
        }
    }
}
