﻿using System;
using System.Collections.Generic;

using SpiceSharp.ParameterSets;

namespace SpiceSharp.Simulations.Frequency
{
    /// <summary>
    /// Rule parameters for a <see cref="Rules"/>.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class RuleParameters : ParameterSet, ICloneable<RuleParameters>
    {
        /// <summary>
        /// Gets the frequencies.
        /// </summary>
        /// <value>
        /// The frequencies.
        /// </value>
        public IEnumerable<double> Frequencies { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleParameters"/> class.
        /// </summary>
        /// <param name="frequencies">The frequencies.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="frequencies"/> is <c>null</c>.</exception>
        public RuleParameters(IEnumerable<double> frequencies)
        {
            Frequencies = frequencies.ThrowIfNull(nameof(frequencies));
        }

        /// <inheritdoc/>
        public RuleParameters Clone()
        {
            return (RuleParameters)MemberwiseClone();
        }
    }
}
