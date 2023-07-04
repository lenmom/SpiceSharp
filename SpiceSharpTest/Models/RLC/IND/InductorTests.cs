using System;
using System.Linq;
using System.Numerics;

using NUnit.Framework;

using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using SpiceSharp.Validation;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class InductorTests : Framework
    {
        [Test]
        public void When_LowpassRLOP_Expect_Reference()
        {
            /*
             * Lowpass RL circuit
             * The inductor should act like an short circuit
             */
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "IN", "0", 1.0),
                new Inductor("L1", "IN", "OUT", 1e-3),
                new Resistor("R1", "OUT", "0", 1.0e3));

            // Create simulation
            OP op = new OP("op");

            // Create exports
            IExport<double>[] exports = new IExport<double>[1];
            exports[0] = new RealVoltageExport(op, "OUT");

            // Create references
            double[] references = { 1.0 };

            // Run test
            AnalyzeOp(op, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_LowpassRLSmallSignal_Expect_Reference()
        {
            /*
             * Lowpass RL filter in the frequency domain should have a single pole at s=-2pi*R/L
             */
            // Create circuit
            double resistance = 1;
            double inductance = 1e-3;
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "IN", "0", 0.0)
                    .SetParameter("acmag", 1.0),
                new Inductor("L1", "IN", "OUT", inductance),
                new Resistor("R1", "OUT", "0", resistance));

            // Create simulation
            AC ac = new AC("ac", new DecadeSweep(0.1, 1e6, 10));

            // Create exports
            IExport<Complex>[] exports = new IExport<Complex>[1];
            exports[0] = new ComplexVoltageExport(ac, "OUT");

            // Create references
            Func<double, Complex>[] references = { f => 1.0 / new Complex(1.0, inductance / resistance * 2 * Math.PI * f) };

            // Run test
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_LCTankTransient_Expect_Reference()
        {
            /*
             * Test for LC tank circuit, an inductor parallel with a capacitor will resonate at a frequency of 1/(2*pi*sqrt(LC))
             */
            // Build circuit
            double capacitance = 1e-3;
            double inductance = 1e-6;
            double initialCurrent = 1e-3;
            Circuit ckt = new Circuit(
                new Inductor("L1", "OUT", "0", inductance)
                    .SetParameter("ic", initialCurrent),
                new Capacitor("C1", "OUT", "0", capacitance)
                );

            /*
             * WARNING: An LC tank is a circuit that oscillates and does not converge. This causes the global truncation error
             * to increase as time goes by!
             * For this reason, the absolute tolerance is made a little bit less strict.
             */
            AbsTol = 1e-9;

            // Create simulation
            Transient tran = new Transient("tran", 1e-9, 1e-3, 1e-7);
            tran.TimeParameters.InitialConditions["OUT"] = 0.0;
            tran.TimeParameters.UseIc = true;

            // Create exports
            IExport<double>[] exports = new IExport<double>[1];
            exports[0] = new RealPropertyExport(tran, "C1", "v");

            // Create reference function
            double amplitude = Math.Sqrt(inductance / capacitance) * initialCurrent;
            double omega = 1.0 / Math.Sqrt(inductance * capacitance);
            Func<double, double>[] references = { t => -amplitude * Math.Sin(omega * t) };

            // Run test
            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_InductorLoop_Expect_SimulationValidationFailedException()
        {
            Circuit ckt = new Circuit(
                new CurrentSource("I1", "in", "0", 1e-3),
                new Inductor("L1", "in", "0", 1e-6),
                new Inductor("L2", "in", "0", 1e-6));

            OP op = new OP("op");
            ValidationFailedException ex = Assert.Throws<ValidationFailedException>(() => op.Run(ckt));
            Assert.AreEqual(1, ex.Rules.ViolationCount);
            IRuleViolation violation = ex.Rules.Violations.First();
            Assert.IsInstanceOf<VoltageLoopRuleViolation>(violation);
        }

        [Test]
        public void When_InductorIC_Expect_Reference()
        {
            double L = 1.0, R = 1.0e3, i0 = 1.0;
            double tau = L / R;

            Circuit ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new Inductor("L1", "in", "out", L).SetParameter("ic", i0),
                new Resistor("R1", "out", "0", R));

            // Make small timesteps for the transient simulation to ensure small truncation errors
            Transient tran = new Transient("tran", new Trapezoidal() { StopTime = 1e-3, MaxStep = 1e-6 });
            tran.TimeParameters.UseIc = true;
            IExport<double>[] exports = new IExport<double>[] { new RealVoltageExport(tran, "out") };
            Func<double, double>[] references = new Func<double, double>[] { time => time > 0 ? i0 * R * Math.Exp(-time / tau) : 0.0 };

            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);
        }
    }
}
