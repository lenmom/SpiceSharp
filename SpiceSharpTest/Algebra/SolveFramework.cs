﻿using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using SpiceSharp.Algebra;

namespace SpiceSharpTest.Algebra
{
    public class SolveFramework
    {
        /// <summary>
        /// Read a file for matrices.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="filename">The filename.</param>
        protected static void ReadMatrix(SparseRealSolver solver, string filename)
        {
            using StreamReader sr = new StreamReader(filename);

            // The first line is a comment
            sr.ReadLine();

            // The second line tells us the dimensions
            string line = sr.ReadLine() ?? throw new Exception("Invalid Mtx file");
            Match match = Regex.Match(line, @"^(?<rows>\d+)\s+(?<columns>\d+)\s+(\d+)");
            int size = int.Parse(match.Groups["rows"].Value);
            if (int.Parse(match.Groups["columns"].Value) != size)
            {
                throw new Exception("Matrix is not square");
            }

            // All subsequent lines are of the format [row] [column] [value]
            while (!sr.EndOfStream)
            {
                // Read the next line
                line = sr.ReadLine();
                if (line == null)
                {
                    break;
                }

                match = Regex.Match(line, @"^(?<row>\d+)\s+(?<column>\d+)\s+(?<value>.*)\s*$");
                if (!match.Success)
                {
                    throw new Exception("Could not recognize file");
                }

                int row = int.Parse(match.Groups["row"].Value);
                int column = int.Parse(match.Groups["column"].Value);
                double value = double.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);

                // Set the value in the matrix
                solver.GetElement(new MatrixLocation(row, column)).Value = value;
            }
        }

        /// <summary>
        /// Reads a file for vectors.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="filename">The filename.</param>
        protected static void ReadRhs(SparseRealSolver solver, string filename)
        {
            using StreamReader sr = new StreamReader(filename);

            // The first line is a comment
            sr.ReadLine();

            // The second line tells us the dimensions
            string line = sr.ReadLine() ?? throw new Exception("Invalid Mtx file");
            Match match = Regex.Match(line, @"^(?<rows>\d+)\s+(\d+)");
            int size = int.Parse(match.Groups["rows"].Value);

            // All subsequent lines are of the format [row] [column] [value]
            while (!sr.EndOfStream)
            {
                // Read the next line
                line = sr.ReadLine();
                if (line == null)
                {
                    break;
                }

                match = Regex.Match(line, @"^(?<row>\d+)\s+(?<value>.*)\s*$");
                if (!match.Success)
                {
                    throw new Exception("Could not recognize file");
                }

                int row = int.Parse(match.Groups["row"].Value);
                double value = double.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);

                // Set the value in the matrix
                solver.GetElement(row).Value = value;
            }
        }

        /// <summary>
        /// Reads a file for a vector.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <param name="filename">The filename.</param>
        protected static void ReadVector(IVector<double> vector, string filename)
        {
            using StreamReader sr = new StreamReader(filename);

            // The first line is a comment
            sr.ReadLine();

            // The second line tells us the dimensions
            string line = sr.ReadLine() ?? throw new Exception("Invalid Mtx file");
            Match match = Regex.Match(line, @"^(?<rows>\d+)\s+(\d+)");
            int size = int.Parse(match.Groups["rows"].Value);

            // All subsequent lines are of the format [row] [column] [value]
            while (!sr.EndOfStream)
            {
                // Read the next line
                line = sr.ReadLine();
                if (line == null)
                {
                    break;
                }

                match = Regex.Match(line, @"^(?<row>\d+)\s+(?<value>.*)\s*$");
                if (!match.Success)
                {
                    throw new Exception("Could not recognize file");
                }

                int row = int.Parse(match.Groups["row"].Value);
                double value = double.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);

                // Set the value in the matrix
                vector[row] = value;
            }
        }

        /// <summary>
        /// Reads a matrix file generated by Spice 3f5.
        /// </summary>
        /// <param name="matFilename">The matrix filename.</param>
        /// <param name="vecFilename">The vector filename.</param>
        /// <returns></returns>
        protected static SparseRealSolver ReadSpice3f5File(string matFilename, string vecFilename)
        {
            SparseRealSolver solver = new SparseRealSolver();

            // Read the spice file
            string line;
            using (StreamReader reader = new StreamReader(matFilename))
            {
                // The file is organized using (row) (column) (value) (imag value)
                while (!reader.EndOfStream && (line = reader.ReadLine()) != null)
                {
                    if (line == "first")
                    {
                        continue;
                    }

                    // Try to read an element
                    Match match = Regex.Match(line, @"^(?<row>\d+)\s+(?<col>\d+)\s+(?<value>[^\s]+)(\s+[^\s]+)?$");
                    if (match.Success)
                    {
                        int row = int.Parse(match.Groups["row"].Value);
                        int col = int.Parse(match.Groups["col"].Value);
                        double value = double.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);
                        solver.GetElement(new MatrixLocation(row, col)).Value = value;
                    }
                }
            }

            // Read the vector file
            using (StreamReader reader = new StreamReader(vecFilename))
            {
                int index = 1;
                while (!reader.EndOfStream && (line = reader.ReadLine()) != null)
                {
                    double value = double.Parse(line, CultureInfo.InvariantCulture);
                    solver.GetElement(index).Value = value;
                    index++;
                }
            }

            return solver;
        }
    }
}
