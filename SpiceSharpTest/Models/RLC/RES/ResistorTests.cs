﻿using System;
using System.Numerics;

using NUnit.Framework;

using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class ResistorTests : Framework
    {
        [Test]
        public void When_SmallResistor_Expect_Reference()
        {
            Circuit ckt = new Circuit(
                new Resistor("R1", "in", "0", 0.0),
                new CurrentSource("I1", "in", "0", 1));
            OP op = new OP("op");
            op.AfterLoad += (sender, args) =>
            {
                SpiceSharp.Algebra.ISparsePivotingSolver<double> solver = op.GetState<IBiasingSimulationState>().Solver;
                SpiceSharp.Algebra.Element<double> elt = solver.FindElement(new SpiceSharp.Algebra.MatrixLocation(1, 1));
                Assert.AreEqual(1.0 / SpiceSharp.Components.Resistors.Parameters.MinimumResistance, elt.Value, 1e-20);
            };
            op.Run(ckt);
        }

        // TODO: Clone
        /*
        [Test]
        public void When_ClonedResistor_Expect_Original()
        {
            var resistor = new Resistor("R1", "a", "b", 1.0e3);
            resistor.Parameters.SeriesMultiplier = 2.0;
            resistor.Parameters.ParallelMultiplier = 3.0;
            var clone = Factory<string>.Get<Resistor>("R1Clone");

            resistor.Parameters.ParallelMultiplier = 1.0;

            Assert.AreEqual(clone.Parameters.Resistance, 1.0e3, 1e-20);
            Assert.AreEqual((double)clone.Parameters.SeriesMultiplier, 2.0, 1e-20);
            Assert.AreEqual((double)clone.Parameters.ParallelMultiplier, 3.0, 1e-20);
        }
        */

        [Test]
        public void When_ResistorModel_Expect_Reference()
        {
            // Bug found by Marcin Golebiowski
            ResistorModel model;
            Resistor res;
            Circuit ckt = new Circuit(
                new CurrentSource("I1", "0", "a", 1),
                model = new ResistorModel("RM1"),
                res = new Resistor("R1", "a", "0", "RM1"));
            model.Parameters.DefaultWidth = 0.5e-6;
            model.Parameters.Narrow = 0.1e-6;
            model.Parameters.SheetResistance = 20;
            res.Parameters.Length = 5e-6;

            OP op = new OP("op");
            IExport<double>[] exports = new IExport<double>[] { new RealVoltageExport(op, "a") };
            double[] references = new[] { 245.0 };

            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        /// <summary>
        /// Create a voltage source shunted by a resistor
        /// </summary>
        /// <param name="dcVoltage">DC voltage</param>
        /// <param name="resistance">Resistance</param>
        /// <returns></returns>
        private static Circuit CreateResistorDcCircuit(double dcVoltage, double resistance)
        {
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "IN", "0", dcVoltage),
                new Resistor("R1", "IN", "0", resistance)
            );
            return ckt;
        }

        [Test]
        public void When_SimpleOP_Expect_Reference()
        {
            /*
             * A circuit contains a DC voltage source 10V and resistor 1000 Ohms
             * The test verifies that after OP simulation:
             * 1) a current through resistor is 0.01 A (Ohms law)
             */
            Circuit ckt = CreateResistorDcCircuit(10, 1000);

            // Create simulation, exports and references
            OP op = new OP("op");
            IExport<double>[] exports = new IExport<double>[1];
            exports[0] = new RealCurrentExport(op, "V1");
            double[] references = { -0.01 };

            // Run
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_DividerSmallSignal_Expect_Reference()
        {
            /*
             * A circuit contains a DC voltage source 10V and resistor 1000 Ohms
             * The test verifies that after AC simulation:
             * 1) a current through resistor is 0.01 A (Ohms law)
             */
            Circuit ckt = CreateResistorDcCircuit(10, 1000);
            ckt["V1"].SetParameter("acmag", 1.0);

            // Create simulation, exports and references
            AC ac = new AC("ac", new LinearSweep(1.0, 10001, 10));
            IExport<Complex>[] exports = { new ComplexPropertyExport(ac, "R1", "i") };
            Func<double, Complex>[] references = { f => 1e-3 };
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        /// <summary>
        /// Create a voltage divider circuit
        /// </summary>
        /// <param name="dcVoltage">DC voltage</param>
        /// <param name="resistance1">Resistance 1</param>
        /// <param name="resistance2">Resistance 2</param>
        /// <returns></returns>
        private static Circuit CreateVoltageDividerResistorDcCircuit(double dcVoltage, double resistance1, double resistance2)
        {
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "IN", "0", dcVoltage),
                new Resistor("R1", "IN", "OUT", resistance1),
                new Resistor("R2", "OUT", "0", resistance2)
            );
            return ckt;
        }

        [Test]
        public void When_DividerOP_Expect_Reference()
        {
            /*
             * A circuit contains a DC voltage source 100V and two resistors in series (1 and 3 Ohms). 
             * It's a voltage divider.
             * The test verifies that after OP simulation:
             * 1) voltage at "OUT" node is ((R1 + R2) / (R1 * R2)) * V 
             */
            Circuit ckt = CreateVoltageDividerResistorDcCircuit(100, 3, 1);

            // Create simulation, exports and references
            OP op = new OP("op");
            IExport<double>[] exports = { new RealPropertyExport(op, "R2", "v") };
            double[] references = { 100.0 * 1.0 / (3.0 + 1.0) };

            // Run
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        /// <summary>
        /// Create a voltage source shunted by two resistors
        /// </summary>
        /// <param name="dcVoltage">DC voltage</param>
        /// <param name="resistance1">Resistance 1</param>
        /// <param name="resistance2">Resistance 2</param>
        /// <returns></returns>
        private static Circuit CreateParallelResistorsDcCircuit(double dcVoltage, double resistance1, double resistance2)
        {
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "IN", "0", dcVoltage),
                new Resistor("R1", "IN", "0", resistance1),
                new Resistor("R2", "IN", "0", resistance2)
            );
            return ckt;
        }

        [Test]
        public void When_ParallelCircuitOP_Expect_Reference()
        {
            /*
             * A circuit contains a DC voltage source 100V and two resistors in parallel (1 and 2 Ohms). 
             * The test verifies that after OP simulation:
             * 1) Current through resistors is 50 and 100A respectively
             */
            double dc = 100;
            double r1 = 2.0;
            double r2 = 1.0;
            Circuit ckt = CreateParallelResistorsDcCircuit(dc, r1, r2);

            // Create simulation, exports and references
            OP op = new OP("op");
            IExport<double>[] exports = { new RealPropertyExport(op, "R1", "i"), new RealPropertyExport(op, "R2", "i") };
            double[] references = { dc / r1, dc / r2 };

            // Run
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_ParameterAsked_Expect_Reference()
        {
            Circuit ckt = new Circuit(
                new Resistor("R1", "out", "0", 1.0e3),
                new Resistor("R2", "in", "0", 10e3),
                new VoltageSource("V1", "in", "0", 1.0));

            // Create simulation exports and references
            OP op = new OP("op");
            IExport<double>[] exports =
            {
                new RealPropertyExport(op, "R1", "resistance"),
                new RealPropertyExport(op, "V1", "dc"),
            };
            double[] references = { 1.0e3, 1.0 };

            // Run
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_MultipliersOp_Expect_Reference()
        {
            Circuit cktActual = new Circuit(
                new VoltageSource("V1", "in", "0", 4.0),
                new Resistor("R1", "in", "0", 1e3).SetParameter("m", 3.0).SetParameter("n", 2.0)
                );
            Circuit cktReference = new Circuit(
                new VoltageSource("V1", "in", "0", 4.0));
            ParallelSeries(cktReference, name => new Resistor(name, "", "", 1e3), "in", "0", 3, 2);

            OP op = new OP("op");
            IExport<double>[] exports = new IExport<double>[] { new RealCurrentExport(op, "V1") };

            Compare(op, cktReference, cktActual, exports);
            DestroyExports(exports);
        }

        [Test]
        public void When_MultipliersSmallSignal_Expect_Reference()
        {
            Circuit cktActual = new Circuit(
                new VoltageSource("V1", "in", "0", 4.0),
                new Resistor("R1", "in", "0", 1e3).SetParameter("m", 3.0).SetParameter("n", 2.0)
                );
            Circuit cktReference = new Circuit(
                new VoltageSource("V1", "in", "0", 4.0));
            ParallelSeries(cktReference, name => new Resistor(name, "", "", 1e3), "in", "0", 3, 2);

            AC ac = new AC("op", new LinearSweep(0, 10, 2));
            IExport<Complex>[] exports = new IExport<Complex>[] { new ComplexCurrentExport(ac, "V1") };

            Compare(ac, cktReference, cktActual, exports);
            DestroyExports(exports);
        }

        [Test]
        public void When_MultipliersNoise_Expect_Reference()
        {
            Circuit cktActual = new Circuit(
                new VoltageSource("V1", "in", "0", 4.0),
                new Resistor("Rs", "in", "out", 10e3),
                new Resistor("R1", "out", "0", 1e3).SetParameter("m", 3.0).SetParameter("n", 2.0)
                );
            Circuit cktReference = new Circuit(
                new VoltageSource("V1", "in", "0", 4.0),
                new Resistor("Rs", "in", "out", 10e3));
            ParallelSeries(cktReference, name => new Resistor(name, "", "", 1e3), "out", "0", 3, 2);

            Noise noise = new Noise("noise", "V1", "out", new LinearSweep(0, 10, 2));
            IExport<double>[] exports = new IExport<double>[] { new OutputNoiseDensityExport(noise), new InputNoiseDensityExport(noise) };

            Compare(noise, cktReference, cktActual, exports);
            DestroyExports(exports);
        }

        [Test]
        public void When_ResistorNoise_Expect_Reference()
        {
            Circuit ckt = new Circuit(
                new CurrentSource("I1", "in", "0", 1),
                new Resistor("R1", "in", "0", 1e3).SetParameter("temp", 20.0));
            double temp = 20 + Constants.CelsiusKelvin;

            Noise noise = new Noise("noise", "I1", "in", new DecadeSweep(10, 10e9, 10));
            OutputNoiseDensityExport onoise = new OutputNoiseDensityExport(noise);
            InputNoiseDensityExport inoise = new InputNoiseDensityExport(noise);
            noise.ExportSimulationData += (sender, args) =>
            {
                // We expect 4*k*T*R noise variance
                Assert.AreEqual(4 * Constants.Boltzmann * temp * 1e3, onoise.Value, 1e-20);
                Assert.AreEqual(4 * Constants.Boltzmann * temp / 1e3, inoise.Value, 1e-20);
            };
            noise.Run(ckt);
        }
    }
}
