﻿using System;
using System.Collections;
using System.Collections.Generic;

using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;

namespace SpiceSharp.Components.ParallelComponents
{
    /// <summary>
    /// An abstract class with a default implementation for parallel access to solvers
    /// in parallel components.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <typeparam name="S">The base simulation state type.</typeparam>
    /// <seealso cref="ISolverSimulationState{T}" />
    public abstract class ParallelSolverState<T, S> : ISolverSimulationState<T>
        where S : ISolverSimulationState<T>
    {
        private readonly ParallelSolver<T> _solver;

        /// <summary>
        /// The parent simulation state.
        /// </summary>
        protected readonly S Parent;

        /// <summary>
        /// Gets the variable with the specified name.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns>The variable.</returns>
        public IVariable<T> this[string name]
        {
            get
            {
                return Parent[name];
            }
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return Parent.Count;
            }
        }

        /// <inheritdoc/>
        public IEqualityComparer<string> Comparer
        {
            get
            {
                return Parent.Comparer;
            }
        }

        /// <summary>
        /// Gets an enumerable collection that contains the keys in the dictionary.
        /// </summary>
        /// <value>
        /// The variable names.
        /// </value>
        public IEnumerable<string> Keys
        {
            get
            {
                return Parent.Keys;
            }
        }

        /// <summary>
        /// Gets an enumerable collection that contains the values in the dictionary.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IEnumerable<IVariable<T>> Values
        {
            get
            {
                return Parent.Values;
            }
        }

        /// <inheritdoc/>
        public ISparsePivotingSolver<T> Solver
        {
            get
            {
                return _solver;
            }
        }

        /// <inheritdoc/>
        public IVector<T> Solution
        {
            get
            {
                return Parent.Solution;
            }
        }

        /// <inheritdoc/>
        public IVariableMap Map
        {
            get
            {
                return Parent.Map;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelSolverState{T, S}"/> class.
        /// </summary>
        /// <param name="parent">The parent simulation state.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parent"/> is <c>null</c>.</exception>
        protected ParallelSolverState(S parent)
        {
            Parent = parent.ThrowIfNull(nameof(parent));
            _solver = new ParallelSolver<T>(parent.Solver);
        }

        /// <summary>
        /// Resets all elements in the common solver.
        /// </summary>
        public void Reset()
        {
            _solver.Reset();
        }

        /// <summary>
        /// Applies the changes to the common solver.
        /// </summary>
        public void Apply()
        {
            _solver.Apply();
        }

        /// <inheritdoc/>
        public IVariable<T> GetSharedVariable(string name)
        {
            return Parent.GetSharedVariable(name);
        }

        /// <inheritdoc/>
        public IVariable<T> CreatePrivateVariable(string name, IUnit unit)
        {
            return Parent.CreatePrivateVariable(name, unit);
        }

        /// <inheritdoc/>
        public void Add(string id, IVariable<T> variable)
        {
            Parent.Add(id, variable);
        }

        /// <summary>
        /// Determines whether the dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>
        ///   <c>true</c> if the dictionary contains an element that has the specified key; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        public bool ContainsKey(string key)
        {
            return Parent.ContainsKey(key);
        }

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        ///   <c>true</c> if the dictionary contains a variable that has the specified key; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        public bool TryGetValue(string key, out IVariable<T> value)
        {
            return Parent.TryGetValue(key, out value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<string, IVariable<T>>> GetEnumerator()
        {
            return Parent.GetEnumerator();
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
