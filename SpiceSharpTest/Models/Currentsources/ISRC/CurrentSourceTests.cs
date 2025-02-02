﻿using System.Collections.Generic;

using NUnit.Framework;

using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class CurrentSourceTests : Framework
    {
        /// <summary>
        /// Creates a circuit with a resistor and a voltage source which is connected to IN
        /// node and a ground node
        /// </summary>
        private static Circuit CreateResistorCircuit(double current, double resistance)
        {
            Circuit ckt = new Circuit(
                new CurrentSource("I1", "0", "IN", current),
                new Resistor("R1", "IN", "0", resistance));
            return ckt;
        }

        [Test]
        public void When_ResistorOP_Expect_Reference()
        {
            /*
             * A circuit contains a current source 10A and resistor 1000 Ohms
             * The test verifies that after OP simulation:
             * 1) a current through resistor is 10A
             * 2) the voltage across the current source is 1000V
             */
            Circuit ckt = CreateResistorCircuit(10, 1.0e3);

            // Create simulation, exports and references
            OP op = new OP("op");
            IExport<double>[] exports = new IExport<double>[2];
            exports[0] = new RealPropertyExport(op, "I1", "v");
            exports[1] = new RealPropertyExport(op, "R1", "i");
            double[] references =
            {
                -10.0e3,
                10
            };

            // Run test
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        /// <summary>
        /// Create a circuit with a current source and N resistors in series to ground
        /// </summary>
        /// <param name="count">Number of resistors to ground</param>
        /// <param name="current">Current (A)</param>
        /// <param name="resistance">Resistance (Ohm)</param>
        /// <returns></returns>
        private static Circuit CreateResistorsInSeriesCircuit(int count, double current, double resistance)
        {
            Assert.IsTrue(count > 1);
            Circuit ckt = new Circuit(
                new CurrentSource("I1", "IN", "0", current),
                new Resistor("R1", "IN", "B1", resistance),
                new Resistor($"R{count}", $"B{count - 1}", "0", resistance)
                );
            for (int i = 2; i <= count - 1; i++)
            {
                ckt.Add(new Resistor($"R{i}", $"B{i - 1}", $"B{i}", resistance));
            }
            return ckt;
        }

        [Test]
        public void When_SeriesResistorOP_Expect_Reference()
        {
            /*
             * A circuit contains a current source 100A and 500 resistor 1000 Ohms
             * The test verifies that after OP simulation:
             * 1) a current through each resistor is 100A
             * 2) a voltage across the current source is 500000V (currentInAmp * resistanceInOhms * resistorCount)
             */
            int currentInAmp = 100;
            int resistanceInOhms = 10;
            int resistorCount = 500;
            Circuit ckt = CreateResistorsInSeriesCircuit(resistorCount, currentInAmp, resistanceInOhms);
            OP op = new OP("op");

            // Create exports
            List<IExport<double>> exports = new List<IExport<double>>();
            for (int i = 1; i <= resistorCount; i++)
            {
                exports.Add(new RealPropertyExport(op, $"R{i}", "i"));
            }

            exports.Add(new RealPropertyExport(op, "I1", "v"));

            // Add references
            List<double> references = new List<double>();
            for (int i = 1; i <= resistorCount; i++)
            {
                references.Add(-100);
            }

            references.Add(-currentInAmp * resistanceInOhms * resistorCount);

            // Run test
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_ResistorTransient_Expect_NoException()
        {
            // Found by Marcin Golebiowski
            Circuit ckt = new Circuit(
                new CurrentSource("I1", "1", "0", new Pulse(0, 6, 3.69e-6, 41e-9, 41e-9, 3.256e-6, 6.52e-6)),
                new Resistor("R1", "1", "0", 10.0)
            );

            Transient tran = new Transient("tran", 1e-8, 1e-5);
            tran.Run(ckt);
        }

        [Test]
        public void When_Cloned_Expect_Reference()
        {
            // Let's check cloning of entities here.
            CurrentSource isrc = (CurrentSource)new CurrentSource("I1", "A", "B", 1.0)
                .SetParameter("ac", new double[] { 1.0, 2.0 })
                .SetParameter("waveform", new Pulse(0.0, 1.0, 1e-9, 1e-8, 1e-7, 1e-6, 1e-5));

            // Clone the entity
            CurrentSource clone = (CurrentSource)isrc.Clone();

            // Change some stuff (should not be reflected in the clone)
            isrc.GetProperty<IWaveformDescription>("waveform").SetParameter("v2", 2.0);

            // Check
            Assert.AreEqual(isrc.Name, clone.Name);
            IReadOnlyList<string> origNodes = isrc.Nodes;
            IReadOnlyList<string> cloneNodes = clone.Nodes;
            Assert.AreEqual(origNodes[0], cloneNodes[0]);
            Assert.AreEqual(origNodes[1], cloneNodes[1]);
            Pulse waveform = (Pulse)clone.GetProperty<IWaveformDescription>("waveform");
            Assert.AreEqual(0.0, waveform.InitialValue, 1e-12);
            Assert.AreEqual(1.0, waveform.PulsedValue, 1e-12);
            Assert.AreEqual(2.0, isrc.GetProperty<IWaveformDescription>("waveform").GetProperty<double>("v2"));
            Assert.AreEqual(1.0, waveform.GetProperty<double>("v2"));
            Assert.AreEqual(1e-5, waveform.GetProperty<double>("per"), 1e-12);
        }

        [Test]
        public void When_ParallelMultiplier_Expect_Reference()
        {
            Circuit ckt_ref = new Circuit(
                new CurrentSource("I1", "out", "0", 1.0),
                new CurrentSource("I2", "out", "0", 1.0),
                new Resistor("R1", "out", "0", 1.0));
            Circuit ckt_act = new Circuit(
                new CurrentSource("I1", "out", "0", 1.0)
                    .SetParameter("m", 2.0),
                new Resistor("R1", "out", "0", 1.0));

            OP op = new OP("op");
            RealVoltageExport[] exports = new[] { new RealVoltageExport(op, "out") };
            Compare(op, ckt_ref, ckt_act, exports);
            DestroyExports(exports);
        }

        [Test]
        public void When_ParallelMultiplierAC_Expect_Reference()
        {
            Circuit ckt_ref = new Circuit(
                new CurrentSource("I1", "ref", "0", 1.0)
                    .SetParameter("ac", new[] { 1.0, 1.0 }),
                new CurrentSource("I2", "ref", "0", 1.0)
                    .SetParameter("ac", new[] { 1.0, 1.0 }),
                new Resistor("R1", "ref", "0", 1.0));
            Circuit ckt_act = new Circuit(
                new CurrentSource("I1", "ref", "0", 1.0)
                    .SetParameter("ac", new[] { 1.0, 1.0 })
                    .SetParameter("m", 2.0),
                new Resistor("R1", "ref", "0", 1.0));

            OP op = new OP("op");
            RealVoltageExport[] exports = new[] { new RealVoltageExport(op, "ref") };
            Compare(op, ckt_ref, ckt_act, exports);
            DestroyExports(exports);
        }
    }
}
