﻿using System;

using SpiceSharp.Attributes;
using SpiceSharp.Components.Bipolars;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="BipolarJunctionTransistor" />.
    /// </summary>
    /// <seealso cref="Entity" />
    /// <seealso cref="IParameterized{P}" />
    /// <seealso cref="ModelParameters" />
    [AutoGeneratedBehaviors]
    public partial class BipolarJunctionTransistorModel : Entity<ModelParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BipolarJunctionTransistorModel"/> class.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public BipolarJunctionTransistorModel(string name)
            : base(name)
        {
        }
    }
}
