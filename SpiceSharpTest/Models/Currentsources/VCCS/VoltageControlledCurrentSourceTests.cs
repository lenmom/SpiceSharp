﻿using System;
using System.Linq;
using System.Numerics;

using NUnit.Framework;

using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class VoltageControlledCurrentSourceTests : Framework
    {
        [Test]
        public void When_VCCSDC_Expect_Reference()
        {
            double transconductance = 2e-3;
            double resistance = 1e4;

            // Build the circuit
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageControlledCurrentSource("G1", "0", "out", "in", "0", transconductance),
                new Resistor("R1", "out", "0", resistance)
                );

            // Make the simulation, exports and references
            DC dc = new DC("DC", "V1", -10.0, 10.0, 1e-3);
            IExport<double>[] exports = { new RealVoltageExport(dc, "out"), new RealPropertyExport(dc, "R1", "i") };
            Func<double, double>[] references = { sweep => sweep * transconductance * resistance, sweep => sweep * transconductance };
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_VCCSSmallSignal_Expect_Reference()
        {
            double magnitude = 0.9;
            double transconductance = 2e-3;
            double resistance = 1e4;

            // Build the circuit
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0)
                    .SetParameter("acmag", magnitude),
                new VoltageControlledCurrentSource("G1", "0", "out", "in", "0", transconductance),
                new Resistor("R1", "out", "0", resistance)
                );

            // Make the simulation, exports and references
            AC ac = new AC("AC", new DecadeSweep(1, 1e4, 3));
            IExport<Complex>[] exports = { new ComplexVoltageExport(ac, "out"), new ComplexPropertyExport(ac, "R1", "i") };
            Func<double, Complex>[] references = { freq => magnitude * transconductance * resistance, freq => magnitude * transconductance };
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_VCCSDC2_Expect_Reference()
        {
            // Found by Marcin Golebiowski
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "1", "0", 200),
                new Resistor("R1", "1", "0", 10),
                new VoltageControlledCurrentSource("G1", "2", "0", "1", "0", 1.5),
                new Resistor("R2", "2", "0", 100));

            OP op = new OP("op1");
            RealPropertyExport current = new RealPropertyExport(op, "G1", "i");
            op.ExportSimulationData += (sender, args) => Assert.AreEqual(300.0, current.Value, 1e-12);
            op.Run(ckt);
            current.Destroy();
        }

        [Test]
        public void When_FloatingOutput_Expect_SimulationValidationFailedException()
        {
            Circuit ckt = new Circuit(
                new Resistor("R1", "0", "1", 1e3),
                new VoltageControlledCurrentSource("F1", "out", "0", "in", "0", 12.0)
                );

            // Make the simulation and run it
            OP dc = new OP("op");
            ValidationFailedException ex = Assert.Throws<ValidationFailedException>(() => dc.Run(ckt));
            Assert.AreEqual(2, ex.Rules.ViolationCount);
            IRuleViolation[] violations = ex.Rules.Violations.ToArray();
            Assert.IsInstanceOf<FloatingNodeRuleViolation>(violations[0]);
            Assert.AreEqual("out", ((FloatingNodeRuleViolation)violations[0]).FloatingVariable.Name);
            Assert.IsInstanceOf<FloatingNodeRuleViolation>(violations[1]);
            Assert.AreEqual("in", ((FloatingNodeRuleViolation)violations[1]).FloatingVariable.Name);
        }

        [Test]
        public void When_ParallelMultiplier_Expect_Reference()
        {
            Circuit ckt_ref = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new VoltageControlledCurrentSource("F1", "ref", "0", "in", "0", 1.0),
                new VoltageControlledCurrentSource("F2", "ref", "0", "in", "0", 1.0),
                new Resistor("R1", "ref", "0", 1.0));
            Circuit ckt_act = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new VoltageControlledCurrentSource("F1", "ref", "0", "in", "0", 1.0)
                    .SetParameter("m", 2.0),
                new Resistor("R1", "ref", "0", 1.0));

            OP op = new OP("op");
            RealVoltageExport[] exports = new[] { new RealVoltageExport(op, "ref") };
            Compare(op, ckt_ref, ckt_act, exports);
            DestroyExports(exports);
        }

        [Test]
        public void When_ParallelMultiplierAC_Expect_Reference()
        {
            Circuit ckt_ref = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0)
                    .SetParameter("ac", new[] { 1.0, 1.0 }),
                new VoltageControlledCurrentSource("F1", "ref", "0", "in", "0", 1.0),
                new VoltageControlledCurrentSource("F2", "ref", "0", "in", "0", 1.0),
                new Resistor("R1", "ref", "0", 1.0));
            Circuit ckt_act = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0)
                    .SetParameter("ac", new[] { 1.0, 1.0 }),
                new VoltageControlledCurrentSource("F1", "ref", "0", "in", "0", 1.0)
                    .SetParameter("m", 2.0),
                new Resistor("R1", "ref", "0", 1.0));

            AC ac = new AC("ac");
            ComplexVoltageExport[] exports = new[] { new ComplexVoltageExport(ac, "ref") };
            Compare(ac, ckt_ref, ckt_act, exports);
            DestroyExports(exports);
        }
    }
}
