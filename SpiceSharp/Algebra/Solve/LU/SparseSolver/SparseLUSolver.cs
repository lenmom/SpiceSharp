﻿using System;

using SpiceSharp.ParameterSets;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// A base class for sparse linear systems that can be solved using LU decomposition.
    /// Pivoting is controlled by the <see cref="Parameters"/> property. The implementation
    /// is optimized for sparse matrices through the <see cref="ISparseMatrix{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="PivotingSolver{M, V, T}"/>
    /// <seealso cref="ISparsePivotingSolver{T}"/>
    /// <seealso cref="ISparseMatrix{T}"/>
    /// <seealso cref="ISparseVector{T}"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="Markowitz{T}"/>
    public abstract partial class SparseLUSolver<T> : PivotingSolver<ISparseMatrix<T>, ISparseVector<T>, T>,
        ISparsePivotingSolver<T>,
        IParameterized<Markowitz<T>>
    {
        /// <summary>
        /// Number of fill-ins in the matrix generated by the solver.
        /// </summary>
        /// <remarks>
        /// Fill-ins are elements that were auto-generated as a consequence
        /// of the solver trying to solve the matrix. To save memory, this
        /// number should remain small.
        /// </remarks>
        public int Fillins { get; private set; }

        /// <summary>
        /// Gets the pivoting strategy being used.
        /// </summary>
        /// <value>
        /// The pivoting strategy.
        /// </value>
        public Markowitz<T> Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseLUSolver{T}"/> class.
        /// </summary>
        /// <param name="magnitude">The magnitude function.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="magnitude"/> is <c>null</c>.</exception>
        protected SparseLUSolver(Func<T, double> magnitude)
            : base(new SparseMatrix<T>(), new SparseVector<T>())
        {
            Parameters = new Markowitz<T>(magnitude);
            Fillins = 0;
        }

        /// <inheritdoc/>
        public override void Precondition(PreconditioningMethod<ISparseMatrix<T>, ISparseVector<T>, T> method)
        {
            ReorderedMatrix reorderedMatrix = new ReorderedMatrix(this);
            ReorderedVector reorderedVector = new ReorderedVector(this);
            method(reorderedMatrix, reorderedVector);
        }

        /// <inheritdoc/>
        public override bool Factor()
        {
            IsFactored = false;
            int order = Size - Degeneracy;
            for (int step = 1; step <= order; step++)
            {
                ISparseMatrixElement<T> pivot = Matrix.FindDiagonalElement(step);

                // We don't consult the pivoting strategy, we just need to know if we can eliminate this row
                if (pivot == null || Parameters.Magnitude(pivot.Value).Equals(0.0))
                {
                    return false;
                }

                Eliminate(Matrix.FindDiagonalElement(step));
            }
            IsFactored = true;
            return true;
        }

        /// <inheritdoc/>
        public override int OrderAndFactor()
        {
            IsFactored = false;
            int step = 1;
            int order = Size - Degeneracy;
            int max = Size - PivotSearchReduction;

            if (!NeedsReordering)
            {
                // Matrix has been factored before, and reordering is not required
                for (step = 1; step <= order; step++)
                {
                    ISparseMatrixElement<T> pivot = Matrix.FindDiagonalElement(step);
                    if (Parameters.IsValidPivot(pivot, max))
                    {
                        Eliminate(pivot);
                    }
                    else
                    {
                        NeedsReordering = true;
                        break;
                    }
                }

                if (!NeedsReordering)
                {
                    IsFactored = true;
                    return order;
                }
            }

            // Setup the strategy for some real kick-ass pivoting action
            Parameters.Setup(Matrix, Vector, step, max);

            for (; step <= order; step++)
            {
                Pivot<ISparseMatrixElement<T>> pivot;
                if (step <= max)
                {
                    pivot = Parameters.FindPivot(Matrix, step, max);
                }
                else
                {
                    ISparseMatrixElement<T> elt = Matrix.FindDiagonalElement(step);
                    pivot = new Pivot<ISparseMatrixElement<T>>(elt, elt != null ? PivotInfo.Good : PivotInfo.None);
                }
                if (pivot.Info == PivotInfo.None)
                {
                    return step - 1;
                }
                else if (pivot.Info == PivotInfo.Bad)
                {
                    MatrixLocation loc = InternalToExternal(new MatrixLocation(step, step));
                    SpiceSharpWarning.Warning(this, Properties.Resources.Algebra_BadlyConditioned.FormatString(loc.Row, loc.Column));
                }
                MovePivot(pivot.Element, step);
                Eliminate(pivot.Element);
            }

            IsFactored = true;
            NeedsReordering = false;
            return order;
        }

        /// <summary>
        /// Eliminates the matrix right and below the pivot.
        /// </summary>
        /// <param name="pivot">The pivot element.</param>
        /// <returns>
        /// <c>true</c> if the elimination was successful; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="AlgebraException">Thrown if the pivot is <c>null</c> or has a magnitude of zero.</exception>
        protected abstract void Eliminate(ISparseMatrixElement<T> pivot);

        /// <summary>
        /// Finds the diagonal element at the specified row/column.
        /// </summary>
        /// <param name="index">The row/column index.</param>
        /// <returns>
        /// The matrix element.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative.</exception>
        public Element<T> FindDiagonalElement(int index)
        {
            index.GreaterThanOrEquals(nameof(index), 0);
            if (index > Size)
            {
                return null;
            }

            int row = Row[index];
            int column = Column[index];
            return Matrix.FindElement(new MatrixLocation(row, column));
        }

        /// <inheritdoc/>
        public Element<T> FindElement(MatrixLocation location)
        {
            return Matrix.FindElement(ExternalToInternal(location));
        }

        /// <inheritdoc/>
        public Element<T> FindElement(int row)
        {
            return Vector.FindElement(Row[row]);
        }

        /// <inheritdoc/>
        public Element<T> GetElement(MatrixLocation location)
        {
            location = ExternalToInternal(location);
            Element<T> elt = Matrix.GetElement(location);

            // If we created a new row or column, let's move to the front
            // to keep the same equations that are linearly dependent
            if (Degeneracy > 0 && Size - Degeneracy > 0)
            {
                if (location.Row == Size)
                {
                    SwapRows(Size, Size - Degeneracy);
                }

                if (location.Column == Size)
                {
                    SwapColumns(Size, Size - Degeneracy);
                }
            }
            return elt;
        }

        /// <inheritdoc/>
        public bool RemoveElement(MatrixLocation location)
        {
            location = ExternalToInternal(location);
            return Matrix.RemoveElement(location);
        }

        /// <inheritdoc/>
        public Element<T> GetElement(int row)
        {
            if (row < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            row = Row[row];
            Element<T> elt = Vector.GetElement(row);

            // If we created a new row, let's move it back to still have the same equations that are considered linearly dependent
            if (Degeneracy > 0)
            {
                if (row == Size)
                {
                    SwapRows(Size, Size - Degeneracy);
                }
            }
            return elt;
        }

        /// <inheritdoc/>
        public bool RemoveElement(int row)
        {
            row = Row[row];
            return Vector.RemoveElement(row);
        }

        /// <summary>
        /// Moves a chosen pivot to the diagonal.
        /// </summary>
        /// <param name="pivot">The pivot element.</param>
        /// <param name="step">The current step of factoring.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="pivot"/> is <c>null</c>.</exception>
        protected void MovePivot(ISparseMatrixElement<T> pivot, int step)
        {
            pivot.ThrowIfNull(nameof(pivot));
            Parameters.MovePivot(Matrix, Vector, pivot, step);

            // Move the pivot in the matrix
            SwapRows(pivot.Row, step);
            SwapColumns(pivot.Column, step);

            // Update the pivoting strategy
            Parameters.Update(Matrix, pivot, Size - PivotSearchReduction);
        }

        /// <summary>
        /// Creates a fillin. The fillin is an element that appeared as a by-product
        /// of elimination/factoring the matrix.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>The created fillin element.</returns>
        protected ISparseMatrixElement<T> CreateFillin(MatrixLocation location)
        {
            ISparseMatrixElement<T> result = (ISparseMatrixElement<T>)Matrix.GetElement(location);
            Parameters.CreateFillin(Matrix, result);
            Fillins++;
            return result;
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            base.Clear();
            Fillins = 0;
            PivotSearchReduction = 0;
            Parameters.Clear();
        }
    }
}
