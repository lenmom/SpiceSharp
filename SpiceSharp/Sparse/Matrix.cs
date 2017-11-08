﻿using System;
using System.Numerics;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Possible errors for sparse matrices
    /// </summary>
    public enum SparseError
    {
        Okay = 0,
        SmallPivot = 0,
        ZeroDiagonal = 102,
        Singular = 102,
        Panic = 101,
        Fatal = 101
    }

    /// <summary>
    /// Possible partitioning method
    /// </summary>
    public enum SparsePartition
    {
        Default = 0,
        Direct = 1,
        Indirect = 2,
        Auto = 3
    }

    /// <summary>
    /// A sparse matrix representation for SpiceSharp
    /// </summary>
    public class Matrix
    {
        /// <summary>
        /// Constants
        /// </summary>
        internal const double DEFAULT_THRESHOLD = 1.0e-3;
        internal const bool DIAG_PIVOTING_AS_DEFAULT = true;
        internal const int MINIMUM_ALLOCATED_SIZE = 6;
        internal const float EXPANSION_FACTOR = 1.5f;
        internal const int MAX_MARKOWITZ_TIES = 100;
        internal const int TIES_MULTIPLIER = 5;
        internal const SparsePartition DEFAULT_PARTITION = SparsePartition.Auto;

        /// <summary>
        /// Flag for indicating if he matrix uses complex numbers or not
        /// </summary>
        public bool Complex { get; set; }

        /// <summary>
        /// Gets the number of fillins
        /// </summary>
        public int Fillins { get; internal set; }

        /// <summary>
        /// Gets the number of elements
        /// </summary>
        public int Elements { get; internal set; }

        /// <summary>
        /// Gets the public size of the matrix
        /// </summary>
        public int ExtSize { get; internal set; }

        /// <summary>
        /// Internal variables
        /// </summary>
        internal double AbsThreshold;
        internal double RelThreshold;

        internal int AllocatedSize;
        internal int AllocatedExtSize;
        internal int CurrentSize;
        internal int IntSize;

        internal MatrixElement[] Diag;
        internal MatrixElement[] FirstInCol;
        internal MatrixElement[] FirstInRow;
        internal MatrixElement TrashCan;

        internal int[] IntToExtColMap;
        internal int[] IntToExtRowMap;
        internal int[] ExtToIntColMap;
        internal int[] ExtToIntRowMap;

        internal bool[] DoCmplxDirect;
        internal bool[] DoRealDirect;
        internal int[] MarkowitzRow;
        internal int[] MarkowitzCol;
        internal long[] MarkowitzProd;

        internal bool Factored;

        internal ElementValue[] Intermediate;
        internal bool InternalVectorsAllocated;

        internal int MaxRowCountInLowerTri;
        internal bool NeedsOrdering;
        internal bool NumberOfInterchangesIsOdd;
        internal bool Partitioned;
        internal int PivotsOriginalCol;
        internal int PivotsOriginalRow;
        internal char PivotSelectionMethod;
        internal bool Reordered;
        internal bool RowsLinked;

        internal int Singletons;

        internal SparseError Error;
        internal int SingularCol;
        internal int SingularRow;

        /// <summary>
        /// Constructor
        /// </summary>
        public Matrix() : this(0, true)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="IntSize">Matrix size</param>
        /// <param name="Complex">Is complex</param>
        public Matrix(int size, bool complex)
        {
            if (size < 0)
                throw new SparseException("Invalid size");

            // Create matrix
            int allocated = Math.Max(size, MINIMUM_ALLOCATED_SIZE);
            int sizeplusone = allocated + 1;

            // Initialize matrix
            Complex = complex;
            Factored = false;
            Elements = 0;
            Error = SparseError.Okay;
            Fillins = 0;
            Reordered = false;
            NeedsOrdering = true;
            NumberOfInterchangesIsOdd = false;
            Partitioned = false;
            RowsLinked = false;
            InternalVectorsAllocated = false;
            SingularCol = 0;
            SingularRow = 0;
            IntSize = size;
            AllocatedSize = allocated;
            ExtSize = size;
            AllocatedExtSize = allocated;
            CurrentSize = 0;
            ExtToIntColMap = null;
            ExtToIntRowMap = null;
            IntToExtColMap = null;
            IntToExtRowMap = null;
            MarkowitzRow = null;
            MarkowitzCol = null;
            MarkowitzProd = null;
            DoCmplxDirect = null;
            DoRealDirect = null;
            Intermediate = null;
            RelThreshold = DEFAULT_THRESHOLD;
            AbsThreshold = 0.0;

            // Take out the trash
            TrashCan = new MatrixElement(0, 0);

            // Allocate space in memory for Diag pointer vector
            Diag = new MatrixElement[sizeplusone];

            // Allocate space in memory for FirstInRow/Col pointer vectors
            FirstInCol = new MatrixElement[sizeplusone];
            FirstInRow = new MatrixElement[sizeplusone];

            // Allocate space in memory for translation vectors
            IntToExtColMap = new int[sizeplusone];
            IntToExtRowMap = new int[sizeplusone];
            ExtToIntColMap = new int[sizeplusone];
            ExtToIntRowMap = new int[sizeplusone];

            // Initialize MapIntToExt vectors
            for (int I = 1; I <= allocated; I++)
            {
                IntToExtRowMap[I] = I;
                IntToExtColMap[I] = I;
                ExtToIntColMap[I] = -1;
                ExtToIntRowMap[I] = -1;
            }
            ExtToIntColMap[0] = 0;
            ExtToIntRowMap[0] = 0;
        }

        /// <summary>
        /// Where is matrix singular
        /// </summary>
        /// <param name="pRow">Row</param>
        /// <param name="pCol">Column</param>
        internal void SingularAt(out int pRow, out int pCol)
        {
            if (Error == SparseError.Singular || Error == SparseError.ZeroDiagonal)
            {
                pRow = SingularRow;
                pCol = SingularCol;
            }
            else
            {
                pRow = 0;
                pCol = 0;
            }
            return;
        }

        /// <summary>
        /// Convert the matrix to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparseOutput.Print(this, false, true, false);
        }
    }
}
