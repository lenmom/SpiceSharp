﻿using System;
using System.Collections;
using System.Collections.Generic;

using SpiceSharp.ParameterSets;
using SpiceSharp.Validation;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// A wrapper for handling rules with subcircuits.
    /// </summary>
    /// <seealso cref="IRules" />
    public class SubcircuitRules : ParameterSetCollection, IRules
    {
        private readonly IRules _parent;
        private readonly ComponentRuleParameters _validationParameters, _parentValidationParameters;

        /// <inheritdoc/>
        public int ViolationCount
        {
            get
            {
                return _parent.ViolationCount;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IRuleViolation> Violations
        {
            get
            {
                return _parent.Violations;
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<IParameterSet> ParameterSets
        {
            get
            {
                foreach (IParameterSet ps in _parent.ParameterSets)
                {
                    if (ps == _parentValidationParameters)
                    {
                        yield return _validationParameters;
                    }
                    else
                    {
                        yield return ps;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitRules"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="parameters">The parameters.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parent"/> or <paramref name="parameters"/> is <c>null</c>.</exception>
        /// <exception cref="TypeNotFoundException">Thrown if <paramref name="parent"/> does not define a <see cref="ComponentRuleParameters"/>.</exception>
        public SubcircuitRules(IRules parent, ComponentRuleParameters parameters)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
            _parentValidationParameters = _parent.GetParameterSet<ComponentRuleParameters>();
            _validationParameters = parameters.ThrowIfNull(nameof(parameters));
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _parent.Reset();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IRule> GetEnumerator()
        {
            return _parent.GetEnumerator();
        }

        /// <inheritdoc/>
        public override P GetParameterSet<P>()
        {
            if (_validationParameters is P p)
            {
                return p;
            }

            return _parent.GetParameterSet<P>();
        }

        /// <inheritdoc/>
        public IEnumerable<R> GetRules<R>() where R : IRule
        {
            return _parent.GetRules<R>();
        }

        /// <inheritdoc/>
        public override bool TryGetParameterSet<P>(out P value)
        {
            if (_validationParameters is P p)
            {
                value = p;
                return true;
            }
            return _parent.TryGetParameterSet(out value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
