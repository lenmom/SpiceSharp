using System;
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
    public class CurrentControlledCurrentSourceTests : Framework
    {
        [Test]
        public void When_SimpleDC_Expect_Reference()
        {
            double gain = 0.85;
            double resistance = 1e4;

            // Build the circuit
            Circuit ckt = new Circuit(
                new CurrentSource("I1", "0", "in", 0.0),
                new VoltageSource("V1", "in", "0", 0.0),
                new CurrentControlledCurrentSource("F1", "0", "out", "V1", gain),
                new Resistor("R1", "out", "0", resistance)
                );

            // Make the simulation, exports and references
            DC dc = new DC("DC", "I1", -10.0, 10.0, 1e-3);
            IExport<double>[] exports = { new RealVoltageExport(dc, "out"), new RealPropertyExport(dc, "R1", "i") };
            Func<double, double>[] references = { sweep => sweep * gain * resistance, sweep => sweep * gain };
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleSmallSignal_Expect_Reference()
        {
            double magnitude = 0.6;
            double gain = 0.85;
            double resistance = 1e4;

            // Build the circuit
            Circuit ckt = new Circuit(
                new CurrentSource("I1", "0", "in", 0.0)
                    .SetParameter("acmag", magnitude),
                new VoltageSource("V1", "in", "0", 0.0),
                new CurrentControlledCurrentSource("F1", "0", "out", "V1", gain),
                new Resistor("R1", "out", "0", resistance)
                );

            // Make the simulation, exports and references
            AC ac = new AC("AC", new DecadeSweep(1, 1e4, 3));
            IExport<Complex>[] exports = { new ComplexVoltageExport(ac, "out"), new ComplexPropertyExport(ac, "R1", "i") };
            Func<double, Complex>[] references = { freq => magnitude * gain * resistance, freq => magnitude * gain };
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_FloatingOutput_Expect_SimulationValidationFailedException()
        {
            Circuit ckt = new Circuit(
                new CurrentSource("I1", "0", "in", 0),
                new VoltageSource("V1", "in", "0", 0),
                new CurrentControlledCurrentSource("F1", "out", "0", "V1", 12.0)
                );

            // Make the simulation and run it
            OP op = new OP("op");
            ValidationFailedException ex = Assert.Throws<ValidationFailedException>(() => op.Run(ckt));
            Assert.AreEqual(1, ex.Rules.ViolationCount);
            IRuleViolation violation = ex.Rules.Violations.First();
            Assert.IsInstanceOf<FloatingNodeRuleViolation>(violation);
            Assert.AreEqual("out", ((FloatingNodeRuleViolation)violation).FloatingVariable.Name);
        }

        [Test]
        public void When_ParallelMultiplier_Expect_Reference()
        {
            Circuit ckt_ref = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("Rref", "in", "0", 1.0),
                new CurrentControlledCurrentSource("F1", "ref", "0", "V1", 1.0),
                new CurrentControlledCurrentSource("F2", "ref", "0", "V1", 1.0),
                new Resistor("R1", "ref", "0", 1.0));
            Circuit ckt_act = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("Rref", "in", "0", 1.0),
                new CurrentControlledCurrentSource("F1", "ref", "0", "V1", 1.0)
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
                new Resistor("Rref", "in", "0", 1.0),
                new CurrentControlledCurrentSource("F1", "out", "0", "V1", 1.0),
                new CurrentControlledCurrentSource("F2", "out", "0", "V1", 1.0),
                new Resistor("R2", "out", "0", 1.0));
            Circuit ckt_act = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0)
                    .SetParameter("ac", new[] { 1.0, 1.0 }),
                new Resistor("Rref", "in", "0", 1.0),
                new CurrentControlledCurrentSource("F1", "out", "0", "V1", 1.0)
                    .SetParameter("m", 2.0),
                new Resistor("R2", "out", "0", 1.0));

            AC ac = new AC("ac");
            ComplexVoltageExport[] exports = new[] { new ComplexVoltageExport(ac, "out") };
            Compare(ac, ckt_ref, ckt_act, exports);
            DestroyExports(exports);
        }
    }
}
