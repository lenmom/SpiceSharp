﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

using NUnit.Framework;

using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models
{
    /// <summary>
    /// Framework for testing models
    /// </summary>
    public class Framework
    {
        protected class NodeMapper : Entity
        {
            private class Mapper : Behavior, IBiasingBehavior
            {
                private readonly List<string> _nodes;
                public Mapper(List<string> nodes, BindingContext context) : base("Mapper")
                {
                    context.ThrowIfNull(nameof(context));
                    _nodes = nodes;
                    IBiasingSimulationState state = context.GetState<IBiasingSimulationState>();
                    _nodes.Select(name => state.GetSharedVariable(name));
                }
                void IBiasingBehavior.Load() { }
            }

            private readonly List<string> _nodes = new List<string>();
            public NodeMapper(params string[] nodes) : base("Mapper")
            {
                _nodes.AddRange(nodes);
            }
            public NodeMapper(IEnumerable<string> nodes) : base("Mapper")
            {
                _nodes.AddRange(nodes);
            }
            public override void CreateBehaviors(ISimulation simulation)
            {
                BehaviorContainer behaviors = new BehaviorContainer(Name);
                BindingContext context = new BindingContext(this, simulation, behaviors);
                behaviors.Add(new Mapper(_nodes, context));
                simulation.EntityBehaviors.Add(behaviors);
            }

            public override IEntity Clone()
            {
                return (IEntity)MemberwiseClone();
            }
        }

        /// <summary>
        /// Absolute tolerance used
        /// </summary>
        public double AbsTol = 1e-12;

        /// <summary>
        /// Relative tolerance used
        /// </summary>
        public double RelTol = 1e-3;

        /// <summary>
        /// Absolute tolerance used for comparing circuits
        /// </summary>
        public double CompareAbsTol = 1e-12;

        /// <summary>
        /// Relative tolerance used for comparing circuits
        /// </summary>
        public double CompareRelTol = 1e-6;

        /// <summary>
        /// Apply a parameter definition to an entity
        /// Parameters are a series of assignments [name]=[value] delimited by spaces.
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="definition">Definition string</param>
        protected static void ApplyParameters(Entity entity, string definition)
        {
            // Get all assignments
            definition = Regex.Replace(definition, @"\s*\=\s*", "=");
            string[] assignments = definition.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string assignment in assignments)
            {
                // Get the name and value
                string[] parts = assignment.Split('=');
                if (parts.Length != 2)
                {
                    throw new Exception("Invalid assignment");
                }

                string name = parts[0].ToLower();
                double value = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

                // Set the entity parameter
                entity.SetParameter(name, value);
            }
        }

        /// <summary>
        /// Creates a subcircuit definition with a component in parallel and series.
        /// </summary>
        public static void ParallelSeries(IEntityCollection ckt, Func<string, IComponent> factory, string ca, string cb, int m, int n)
        {
            for (int j = 0; j < m; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    IComponent clone = factory("entity" + j.ToString() + "_" + i.ToString());
                    string a, b;
                    if (i == 0)
                    {
                        a = ca;
                    }
                    else
                    {
                        a = "n" + j.ToString() + "_" + (i - 1).ToString();
                    }

                    if (i == n - 1)
                    {
                        b = cb;
                    }
                    else
                    {
                        b = "n" + j.ToString() + "_" + i.ToString();
                    }

                    clone.Connect(a, b);
                    ckt.Add(clone);
                }
            }
        }

        /// <summary>
        /// Compares the two circuits for the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="cktReference">The reference circuit.</param>
        /// <param name="cktActual">The actual circuit.</param>
        /// <param name="exports">The exports to be compared.</param>
        public void Compare(IEventfulSimulation simulation, IEntityCollection cktReference, IEntityCollection cktActual, IExport<double>[] exports)
        {
            List<double> results = new List<double>();
            void StoreResults(object sender, ExportDataEventArgs args)
            {
                foreach (IExport<double> export in exports)
                {
                    results.Add(export.Value);
                }
            }
            int index = 0;
            void CompareResults(object sender, ExportDataEventArgs args)
            {
                foreach (IExport<double> export in exports)
                {
                    double expected = results[index++];
                    double actual = export.Value;
                    double tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * CompareRelTol + CompareAbsTol;
                    Assert.AreEqual(expected, actual, tol);
                }
            }

            // Store results
            simulation.ExportSimulationData += StoreResults;
            simulation.Run(cktReference);
            simulation.ExportSimulationData -= StoreResults;

            // Compare to second circuit
            simulation.ExportSimulationData += CompareResults;
            simulation.Run(cktActual);
            simulation.ExportSimulationData -= CompareResults;
        }

        /// <summary>
        /// Compares the two circuits for the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="cktReference">The reference circuit.</param>
        /// <param name="cktActual">The actual circuit.</param>
        /// <param name="exports">The exports to be compared.</param>
        public void Compare(IEventfulSimulation simulation, IEntityCollection cktReference, IEntityCollection cktActual, IExport<Complex>[] exports)
        {
            List<Complex> results = new List<Complex>();
            void StoreResults(object sender, ExportDataEventArgs args)
            {
                foreach (IExport<Complex> export in exports)
                {
                    results.Add(export.Value);
                }
            }
            int index = 0;
            void CompareResults(object sender, ExportDataEventArgs args)
            {
                foreach (IExport<Complex> export in exports)
                {
                    Complex expected = results[index++];
                    Complex actual = export.Value;
                    double tol = Math.Max(Math.Abs(expected.Real), Math.Abs(actual.Real)) * CompareRelTol + CompareAbsTol;
                    Assert.AreEqual(expected.Real, actual.Real, tol);
                    tol = Math.Max(Math.Abs(expected.Imaginary), Math.Abs(actual.Imaginary)) * CompareRelTol + CompareAbsTol;
                    Assert.AreEqual(expected.Imaginary, actual.Imaginary, tol);
                }
            }

            // Store results
            simulation.ExportSimulationData += StoreResults;
            simulation.Run(cktReference);
            simulation.ExportSimulationData -= StoreResults;

            // Compare to second circuit
            simulation.ExportSimulationData += CompareResults;
            simulation.Run(cktActual);
            simulation.ExportSimulationData -= CompareResults;
        }

        /// <summary>
        /// Perform a test for OP analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyzeOp(OP sim, Circuit ckt, IEnumerable<IExport<double>> exports, IEnumerable<double> references)
        {
            if (exports == null)
            {
                throw new ArgumentNullException(nameof(exports));
            }

            if (references == null)
            {
                throw new ArgumentNullException(nameof(references));
            }

            sim.ExportSimulationData += (sender, data) =>
            {
                using IEnumerator<IExport<double>> exportIt = exports.GetEnumerator();
                using IEnumerator<double> referencesIt = references.GetEnumerator();
                while (exportIt.MoveNext() && referencesIt.MoveNext())
                {
                    double actual = exportIt.Current?.Value ?? throw new ArgumentNullException();
                    double expected = referencesIt.Current;
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;
                    Assert.AreEqual(expected, actual, tol);
                }
            };
            sim.Run(ckt);
        }

        /// <summary>
        /// Perform a test for DC analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyzeDC(DC sim, Circuit ckt, IEnumerable<IExport<double>> exports, IEnumerable<double[]> references)
        {
            if (exports == null)
            {
                throw new ArgumentNullException(nameof(exports));
            }

            if (references == null)
            {
                throw new ArgumentNullException(nameof(references));
            }

            int index = 0;
            sim.ExportSimulationData += (sender, data) =>
            {
                using IEnumerator<IExport<double>> exportIt = exports.GetEnumerator();
                using IEnumerator<double[]> referencesIt = references.GetEnumerator();
                while (exportIt.MoveNext() && referencesIt.MoveNext())
                {
                    double actual = exportIt.Current?.Value ?? double.NaN;
                    double expected = referencesIt.Current?[index] ?? double.NaN;
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;

                    try
                    {
                        Assert.AreEqual(expected, actual, tol);
                    }
                    catch (Exception ex)
                    {
                        ICollection<ISweep> sweeps = sim.DCParameters.Sweeps;
                        double[] values = sim.GetCurrentSweepValue();
                        string msg = ex.Message + " at ";
                        int index = 0;
                        foreach (ISweep sweep in sweeps)
                        {
                            msg += "{0}={1}".FormatString(sweep.Name, values[index++]) + ", ";
                        }

                        throw new Exception(msg, ex);
                    }
                }

                index++;
            };
            sim.Run(ckt);
        }

        /// <summary>
        /// Perform a test for DC analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyzeDC(DC sim, Circuit ckt, IEnumerable<IExport<double>> exports, IEnumerable<Func<double, double>> references)
        {
            sim.ExportSimulationData += (sender, data) =>
            {
                using IEnumerator<IExport<double>> exportIt = exports.GetEnumerator();
                using IEnumerator<Func<double, double>> referencesIt = references.GetEnumerator();
                while (exportIt.MoveNext() && referencesIt.MoveNext())
                {
                    double actual = exportIt.Current?.Value ?? throw new ArgumentNullException();
                    double expected = referencesIt.Current?.Invoke(sim.GetCurrentSweepValue()[0]) ?? throw new ArgumentNullException();
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;

                    try
                    {
                        Assert.AreEqual(expected, actual, tol);
                    }
                    catch (Exception ex)
                    {
                        ICollection<ISweep> sweeps = sim.DCParameters.Sweeps;
                        double[] values = sim.GetCurrentSweepValue();
                        string msg = ex.Message + " at ";
                        int index = 0;
                        foreach (ISweep sweep in sweeps)
                        {
                            msg += "{0}={1}".FormatString(sweep.Name, values[index++]) + ", ";
                        }

                        throw new Exception(msg, ex);
                    }
                }
            };
            sim.Run(ckt);
        }

        /// <summary>
        /// Perform a test for AC analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyzeAC(AC sim, Circuit ckt, IEnumerable<IExport<double>> exports, IEnumerable<double[]> references)
        {
            int index = 0;
            sim.ExportSimulationData += (sender, data) =>
            {
                using IEnumerator<IExport<double>> exportsIt = exports.GetEnumerator();
                using IEnumerator<double[]> referencesIt = references.GetEnumerator();
                while (exportsIt.MoveNext() && referencesIt.MoveNext())
                {
                    // Test export
                    double actual = exportsIt.Current?.Value ?? throw new ArgumentNullException();
                    double expected = referencesIt.Current?[index] ?? throw new ArgumentNullException();
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;

                    try
                    {
                        Assert.AreEqual(expected, actual, tol);
                    }
                    catch (Exception ex)
                    {
                        string msg = $"{ex.Message} at {data.Frequency} Hz";
                        throw new Exception(msg, ex);
                    }
                }

                index++;
            };
            sim.Run(ckt);
        }

        /// <summary>
        /// Perform a test for AC analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyzeAC(AC sim, Circuit ckt, IEnumerable<IExport<Complex>> exports, IEnumerable<Complex[]> references)
        {
            int index = 0;
            sim.ExportSimulationData += (sender, data) =>
            {
                using IEnumerator<IExport<Complex>> exportsIt = exports.GetEnumerator();
                using IEnumerator<Complex[]> referencesIt = references.GetEnumerator();
                while (exportsIt.MoveNext() && referencesIt.MoveNext())
                {
                    // Test export
                    Complex actual = exportsIt.Current?.Value ?? throw new ArgumentNullException();
                    Complex expected = referencesIt.Current?[index] ?? throw new ArgumentNullException();

                    // Test real part
                    double rtol = Math.Max(Math.Abs(actual.Real), Math.Abs(expected.Real)) * RelTol + AbsTol;
                    double itol = Math.Max(Math.Abs(actual.Imaginary), Math.Abs(expected.Imaginary)) * RelTol +
                                  AbsTol;

                    try
                    {
                        Assert.AreEqual(expected.Real, actual.Real, rtol);
                        Assert.AreEqual(expected.Imaginary, actual.Imaginary, itol);
                    }
                    catch (Exception ex)
                    {
                        string msg = $"{ex.Message} at {data.Frequency} Hz";
                        throw new Exception(msg, ex);
                    }
                }

                index++;
            };
            sim.Run(ckt);
        }

        /// <summary>
        /// Perform a test for AC analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyzeAC(AC sim, Circuit ckt, IEnumerable<IExport<Complex>> exports, IEnumerable<Func<double, Complex>> references)
        {
            sim.ExportSimulationData += (sender, data) =>
            {
                using IEnumerator<IExport<Complex>> exportsIt = exports.GetEnumerator();
                using IEnumerator<Func<double, Complex>> referencesIt = references.GetEnumerator();
                while (exportsIt.MoveNext() && referencesIt.MoveNext())
                {
                    // Test export
                    Complex actual = exportsIt.Current?.Value ?? throw new ArgumentNullException();
                    Complex expected = referencesIt.Current?.Invoke(data.Frequency) ?? throw new ArgumentNullException();

                    // Test real part
                    double rtol = Math.Max(Math.Abs(actual.Real), Math.Abs(expected.Real)) * RelTol + AbsTol;
                    double itol = Math.Max(Math.Abs(actual.Imaginary), Math.Abs(expected.Imaginary)) * RelTol +
                                  AbsTol;

                    try
                    {
                        Assert.AreEqual(expected.Real, actual.Real, rtol);
                        Assert.AreEqual(expected.Imaginary, actual.Imaginary, itol);
                    }
                    catch (Exception ex)
                    {
                        string msg = $"{ex.Message} at {data.Frequency} Hz";
                        throw new Exception(msg, ex);
                    }
                }
            };
            sim.Run(ckt);
        }

        /// <summary>
        /// Perform a test for transient analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyzeTransient(Transient sim, Circuit ckt, IEnumerable<IExport<double>> exports, IEnumerable<double[]> references)
        {
            int index = 0;
            sim.ExportSimulationData += (sender, data) =>
            {
                using IEnumerator<IExport<double>> exportsIt = exports.GetEnumerator();
                using IEnumerator<double[]> referencesIt = references.GetEnumerator();
                while (exportsIt.MoveNext() && referencesIt.MoveNext())
                {
                    double actual = exportsIt.Current?.Value ?? throw new ArgumentNullException();
                    double expected = referencesIt.Current?[index] ?? throw new ArgumentNullException();
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;

                    try
                    {
                        Assert.AreEqual(expected, actual, tol);
                    }
                    catch (Exception ex)
                    {
                        string msg = $"{ex.Message} at t={data.Time} s";
                        throw new Exception(msg, ex);
                    }
                }

                index++;
            };
            sim.Run(ckt);
        }

        /// <summary>
        /// Perform a test for transient analysis where the reference is a function in time
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyzeTransient(Transient sim, Circuit ckt, IEnumerable<IExport<double>> exports, IEnumerable<Func<double, double>> references)
        {
            sim.ExportSimulationData += (sender, data) =>
            {
                using IEnumerator<IExport<double>> exportsIt = exports.GetEnumerator();
                using IEnumerator<Func<double, double>> referencesIt = references.GetEnumerator();
                while (exportsIt.MoveNext() && referencesIt.MoveNext())
                {
                    double t = data.Time;
                    double actual = exportsIt.Current?.Value ?? throw new ArgumentNullException();
                    double expected = referencesIt.Current?.Invoke(t) ?? throw new ArgumentNullException();
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;

                    try
                    {
                        Assert.AreEqual(expected, actual, tol);
                    }
                    catch (Exception ex)
                    {
                        string msg = $"{ex.Message} at t={data.Time} s";
                        throw new Exception(msg, ex);
                    }
                }
            };
            sim.Run(ckt);
        }

        /// <summary>
        /// Perform a test for noise analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected static void AnalyzeNoise(Noise sim, Circuit ckt, IEnumerable<IExport<double>> exports, IEnumerable<double[]> references)
        {
            int index = 0;
            sim.ExportSimulationData += (sender, data) =>
            {
                using IEnumerator<IExport<double>> exportsIt = exports.GetEnumerator();
                using IEnumerator<double[]> referencesIt = references.GetEnumerator();
                while (exportsIt.MoveNext() && referencesIt.MoveNext())
                {
                    // Test export
                    double actual = exportsIt.Current?.Value ?? throw new ArgumentNullException();
                    double expected = referencesIt.Current?[index] ?? throw new ArgumentNullException();
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-6 + 1e-12;

                    try
                    {
                        Assert.AreEqual(expected, actual, tol);
                    }
                    catch (Exception ex)
                    {
                        string msg = $"{ex.Message} at {data.Frequency} Hz";
                        throw new Exception(msg, ex);
                    }
                }

                index++;
            };
            sim.Run(ckt);
        }

        /// <summary>
        /// Writes the exports to the console window.
        /// Can be used for debugging. The output is in the format:
        /// v0 = [ ... ];
        /// v1 = [ ... ];
        /// ...
        /// </summary>
        /// <param name="sim">The simulation.</param>
        /// <param name="ckt">The circuit.</param>
        /// <param name="exports">The exports.</param>
        protected static void WriteExportsToConsole(Simulation sim, Circuit ckt, IEnumerable<IExport<double>> exports)
        {
            IExport<double>[] arr = exports.ToArray();
            List<string>[] output = new List<string>[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                output[i] = new List<string>();
            }

            sim.ExportSimulationData += (sender, args) =>
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    output[i].Add(arr[i].Value.ToString(CultureInfo.InvariantCulture));
                }
            };

            sim.Run(ckt);

            for (int i = 0; i < arr.Length; i++)
            {
                Console.WriteLine($"v{i} = [{string.Join(", ", output[i])} ];");
            }
        }

        /// <summary>
        /// Destroy all exports.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <param name="exports">The exports.</param>
        protected static void DestroyExports<T>(IEnumerable<IExport<T>> exports)
        {
            foreach (IExport<T> export in exports)
            {
                export.Destroy();
            }
        }

        /// <summary>
        /// Dump transient information in the console (used for debugging).
        /// </summary>
        /// <param name="tran">The transient analysis.</param>
        /// <param name="ckt">The circuit.</param>
        protected static void DumpTransientState(Transient tran, Circuit ckt)
        {
            IIntegrationMethod state = tran.GetState<IIntegrationMethod>();
            IBiasingSimulationState rstate = tran.GetState<IBiasingSimulationState>();

            Console.WriteLine("----------- Dumping transient information -------------");
            Console.WriteLine($"Base time: {state.BaseTime}");
            Console.WriteLine($"Target time: {state.Time}");
            Console.Write($"Last timesteps (current first):");
            for (int i = 0; i <= state.MaxOrder; i++)
            {
                Console.Write("{0}{1}", i > 0 ? ", " : "", state.GetPreviousTimestep(i));
            }

            Console.WriteLine();
            Console.WriteLine("Problem variable: {0}", tran.ProblemVariable);
            Console.WriteLine("Problem variable value: {0}", rstate.Solution[rstate.Map[tran.ProblemVariable]]);
            Console.WriteLine();

            // Dump the circuit contents
            Console.WriteLine("- Circuit contents");
            foreach (IEntity entity in ckt)
            {
                Console.Write(entity.Name);
                if (entity is Component c)
                {
                    foreach (string node in c.Nodes)
                    {
                        Console.Write($"{node} ");
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            // Dump the current iteration solution
            Console.WriteLine("- Solutions");
            Dictionary<int, string> variables = new Dictionary<int, string>();
            foreach (KeyValuePair<IVariable, int> variable in rstate.Map)
            {
                variables.Add(variable.Value, $"{variable.Value} - {variable.Key.Name} ({variable.Key.Unit}): {rstate.Solution[variable.Value]}");
            }

            for (int i = 0; i <= state.MaxOrder; i++)
            {
                SpiceSharp.Algebra.IVector<double> oldsolution = state.GetPreviousSolution(i);
                for (int k = 1; k <= variables.Count; k++)
                {
                    variables[k] += $", {oldsolution[k]}";
                }
            }
            for (int i = 0; i <= variables.Count; i++)
            {
                if (variables.TryGetValue(i, out string value))
                {
                    Console.WriteLine(value);
                }
                else
                {
                    Console.WriteLine($"Could not find variable for index {i}");
                }
            }
            Console.WriteLine();

            /*
            // Dump the states used by the transient
            #if DEBUG
            Console.WriteLine("- States");
            var intstate = state.GetPreviousStates(0);
            string[] output = new string[intstate.Length];
            for (var i = 0; i < intstate.Length; i++)
                output[i] = $"{intstate[i]}";
            for (var k = 1; k <= state.MaxOrder; k++)
            {
                intstate = state.GetPreviousStates(k);
                for (var i = 0; i < intstate.Length; i++)
                    output[i] += $", {intstate[i]}";
            }
            for (var i = 0; i < output.Length; i++)
                Console.WriteLine(output[i]);
            Console.WriteLine();
            #endif
            */

            Console.WriteLine("------------------------ End of information ------------------------");
        }
    }
}
