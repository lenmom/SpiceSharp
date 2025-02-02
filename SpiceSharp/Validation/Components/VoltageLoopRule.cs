﻿using System.Collections.Generic;

using SpiceSharp.Simulations;
using SpiceSharp.Validation.Components;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An implementation for a <see cref="IAppliedVoltageRule"/>. This class
    /// will check for voltage loops.
    /// </summary>
    /// <seealso cref="IAppliedVoltageRule" />
    public class VoltageLoopRule : IAppliedVoltageRule
    {
        private readonly Dictionary<IVariable, Group> _groups = new Dictionary<IVariable, Group>();
        private readonly List<VoltageLoopRuleViolation> _violations = new List<VoltageLoopRuleViolation>();

        /// <summary>
        /// Gets the number of violations of this rule.
        /// </summary>
        /// <value>
        /// The violation count.
        /// </value>
        public int ViolationCount
        {
            get
            {
                return _violations.Count;
            }
        }

        /// <summary>
        /// Gets the rule violations.
        /// </summary>
        /// <value>
        /// The rule violations.
        /// </value>
        public IEnumerable<IRuleViolation> Violations
        {
            get
            {
                foreach (VoltageLoopRuleViolation violation in _violations)
                {
                    yield return violation;
                }
            }
        }

        /// <summary>
        /// Resets the rule.
        /// </summary>
        public void Reset()
        {
            _violations.Clear();
            _groups.Clear();
        }

        /// <summary>
        /// Fixes the voltage difference between two node variables.
        /// </summary>
        /// <param name="subject">The subject that applies to the rule.</param>
        /// <param name="a">The first variable.</param>
        /// <param name="b">The second variable.</param>
        public void Fix(IRuleSubject subject, IVariable a, IVariable b)
        {
            // If both variables are part of the same fixed-voltage group, then this rule is violated
            bool hasA = _groups.TryGetValue(a, out Group groupA);
            bool hasB = _groups.TryGetValue(b, out Group groupB);
            if (hasA && hasB)
            {
                if (groupA == groupB)
                {
                    _violations.Add(new VoltageLoopRuleViolation(this, subject, a, b));
                }
                else
                {
                    foreach (IVariable variable in groupB)
                    {
                        _groups[variable] = groupA;
                    }

                    groupA.Join(groupB);
                }
            }
            else if (hasA)
            {
                _groups.Add(b, groupA);
                groupA.Add(b);
            }
            else if (hasB)
            {
                _groups.Add(a, groupB);
                groupB.Add(a);
            }
            else
            {
                Group group = new Group(a, b);
                _groups.Add(a, group);
                _groups.Add(b, group);
            }
        }
    }
}
