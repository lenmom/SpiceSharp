﻿using System;

using SpiceSharp.Attributes;
using SpiceSharp.Components.Diodes;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="Diode"/>.
    /// </summary>
    /// <seealso cref="Entity"/>
    /// <seealso cref="BindingContext"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="ModelParameters"/>
    [AutoGeneratedBehaviors]
    public partial class DiodeModel : Entity<ModelParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiodeModel"/> class.
        /// </summary>
        /// <param name="name">The name of the diode model.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public DiodeModel(string name)
            : base(name)
        {
        }
    }
}
