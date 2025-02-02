﻿using System;
using System.Linq;

using SpiceSharp.Attributes;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Validation;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent current source.
    /// </summary>
    /// <seealso cref="Component"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="IndependentSourceParameters"/>
    /// <seealso cref="IRuleSubject"/>
    [Pin(0, "I+"), Pin(1, "I-"), IndependentSource, Connected, AutoGeneratedBehaviors]
    public partial class CurrentSource : Component<CurrentSources.Parameters>,
        IRuleSubject
    {
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
        public const int PinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current source.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public CurrentSource(string name)
            : base(name, PinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current source.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="dc">The DC value.</param>
        public CurrentSource(string name, string pos, string neg, double dc)
            : this(name)
        {
            Parameters.DcValue = dc;
            Connect(pos, neg);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current source.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="waveform">The Waveform-object.</param>
        public CurrentSource(string name, string pos, string neg, IWaveformDescription waveform)
            : this(name)
        {
            Parameters.Waveform = waveform;
            Connect(pos, neg);
        }

        /// <inheritdoc/>
        void IRuleSubject.Apply(IRules rules)
        {
            ComponentRuleParameters p = rules.GetParameterSet<ComponentRuleParameters>();
            Simulations.IVariable[] nodes = Nodes.Select(name => p.Factory.GetSharedVariable(name)).ToArray();
            foreach (IConductiveRule rule in rules.GetRules<IConductiveRule>())
            {
                rule.AddPath(this, ConductionTypes.None, nodes[0], nodes[1]);
            }
        }
    }
}
