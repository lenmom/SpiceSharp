﻿using System;

using NUnit.Framework;

using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Documentation;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;

namespace SpiceSharpTest
{
    [TestFixture]
    public class BasicExampleTests
    {
        [Test]
        public void When_BasicResistor_Expect_NoException()
        {
            // <example_structure_resistor>
            // Build the circuit
            Circuit ckt = new Circuit(
                new Resistor("R1", "a", "b", 1e3)
            );

            // Change the value of the resistor
            ckt["R1"].GetParameterSet<SpiceSharp.Components.Resistors.Parameters>().Resistance = 2.0e3;
            // </example_structure_resistor>

            // <example_structure_resistor_2>
            // Using the ParameterNameAttribute
            ckt["R1"].SetParameter("resistance", 2.0e3);
            ckt["R1"].GetParameterSet<SpiceSharp.Components.Resistors.Parameters>().SetParameter("resistance", 2.0e3);
            ((Resistor)ckt["R1"]).Parameters.Resistance = 2.0e3;
            // </example_structure_resistor_2>
        }

        [Test]
        public void When_BasicSimulation_Expect_NoException()
        {
            // <example_structure_dc>
            // Build the simulation
            DC dc = new DC("DC 1");

            // Add a sweep
            dc.DCParameters.Sweeps.Add(new ParameterSweep("V1", new LinearSweep(0.0, 3.3, 0.1)));
            // </example_structure_dc>

            // <example_structure_dc_2>
            dc.BiasingParameters.RelativeTolerance = 1e-4;
            dc.BiasingParameters.AbsoluteTolerance = 1e-10;
            // </example_structure_dc_2>
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "This is example code")]
        public void When_BasicParameters_Expect_NoException()
        {
            // Create the mosfet
            Mosfet1Model model = new Mosfet1Model("M1");
            SpiceSharp.Components.Mosfets.Level1.ModelParameters parameters = model.GetParameterSet<SpiceSharp.Components.Mosfets.Level1.ModelParameters>();

            // <example_parameters_mos1_creategetter>
            // Create a getter for the nominal temperature of the mosfet1 model
            Func<double> tnomGetter = parameters.CreatePropertyGetter<double>("tnom");
            double temperature = tnomGetter(); // In degrees Celsius
            // </example_parameters_mos1_creategetter>

            // <example_parameters_mos1_createsetter>
            // Create a setter for the gate-drain overlap capacitance of the mosfet1 model
            Action<double> cgdoSetter = parameters.CreateParameterSetter<double>("cgdo");
            cgdoSetter(1e-12); // 1pF
            // </example_parameters_mos1_createsetter>

            // <example_parameters_mos1_getparameter>
            // Get the parameter that describes the oxide thickness of the mosfet1 model
            double toxParameter = parameters.GetProperty<double>("tox");
            // </example_parameters_mos1_getparameter>

            // <example_parameters_mos1_setparameter>
            // Flag the model as a PMOS type
            parameters.SetParameter("pmos", true);
            // </example_parameters_mos1_setparameter>
        }

        [Test]
        public void When_BasicCircuit_Expect_NoException()
        {
            // <example01_build>
            // Build the circuit
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "out", 1.0e4),
                new Resistor("R2", "out", "0", 2.0e4)
                );
            // </example01_build>
            // <example01_simulate>
            // Create a DC simulation that sweeps V1 from -1V to 1V in steps of 100mV
            DC dc = new DC("DC 1", "V1", -1.0, 1.0, 0.2);

            // Catch exported data
            dc.ExportSimulationData += (sender, args) =>
            {
                double input = args.GetVoltage("in");
                double output = args.GetVoltage("out");
            };
            dc.Run(ckt);
            // </example01_simulate>
        }

        [Test]
        public void When_BasicCircuitExports_Expect_NoException()
        {
            // Build the circuit
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "out", 1.0e4),
                new Resistor("R2", "out", "0", 2.0e4)
            );

            // <example01_simulate2>
            // Create a DC simulation that sweeps V1 from -1V to 1V in steps of 100mV
            DC dc = new DC("DC 1", "V1", -1.0, 1.0, 0.2);

            // Create exports
            RealVoltageExport inputExport = new RealVoltageExport(dc, "in");
            RealVoltageExport outputExport = new RealVoltageExport(dc, "out");
            RealPropertyExport currentExport = new RealPropertyExport(dc, "V1", "i");

            // Catch exported data
            dc.ExportSimulationData += (sender, args) =>
            {
                double input = inputExport.Value;
                double output = outputExport.Value;
                double current = currentExport.Value;
            };
            dc.Run(ckt);
            // </example01_simulate2>
        }

        [Test]
        public void When_NMOSIVCharacteristic_Expect_NoException()
        {
            // <example_DC>
            // Create the mosfet and its model
            Mosfet1 nmos = new Mosfet1("M1", "d", "g", "0", "0", "example");
            Mosfet1Model nmosmodel = new Mosfet1Model("example");
            nmosmodel.SetParameter("kp", 150.0e-3);

            // Build the circuit
            Circuit ckt = new Circuit(
                new VoltageSource("Vgs", "g", "0", 0),
                new VoltageSource("Vds", "d", "0", 0),
                nmosmodel,
                nmos
                );

            // Sweep the base current and vce voltage
            DC dc = new DC("DC 1", new[]
            {
                new ParameterSweep("Vgs", new LinearSweep(0, 3, 0.2)),
                new ParameterSweep("Vds", new LinearSweep(0, 5, 0.1)),
            });

            // Export the collector current
            RealPropertyExport currentExport = new RealPropertyExport(dc, "M1", "id");

            // Run the simulation
            dc.ExportSimulationData += (sender, args) =>
            {
                double vgsVoltage = dc.GetCurrentSweepValue()[0];
                double vdsVoltage = dc.GetCurrentSweepValue()[1];
                double current = currentExport.Value;
            };
            dc.Run(ckt);
            // </example_DC>
        }

        [Test]
        public void When_RCFilterAC_Expect_NoException()
        {
            // <example_AC>
            // Build the circuit
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0)
                    .SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "out", 10.0e3),
                new Capacitor("C1", "out", "0", 1e-6)
                );

            // Create the simulation
            AC ac = new AC("AC 1", new DecadeSweep(1e-2, 1.0e3, 5));

            // Make the export
            ComplexVoltageExport exportVoltage = new ComplexVoltageExport(ac, "out");

            // Simulate
            ac.ExportSimulationData += (sender, args) =>
            {
                System.Numerics.Complex output = exportVoltage.Value;
                double decibels = 10.0 * Math.Log10(output.Real * output.Real + output.Imaginary * output.Imaginary);
            };
            ac.Run(ckt);
            // </example_AC>
        }

        [Test]
        public void When_RCFilterTransient_Expect_NoException()
        {
            // <example_Transient>
            // Build the circuit
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pulse(0.0, 5.0, 0.01, 1e-3, 1e-3, 0.02, 0.04)),
                new Resistor("R1", "in", "out", 10.0e3),
                new Capacitor("C1", "out", "0", 1e-6)
            );

            // Create the simulation
            Transient tran = new Transient("Tran 1", 1e-3, 0.1);

            // Make the exports
            RealVoltageExport inputExport = new RealVoltageExport(tran, "in");
            RealVoltageExport outputExport = new RealVoltageExport(tran, "out");

            // Simulate
            tran.ExportSimulationData += (sender, args) =>
            {
                double input = inputExport.Value;
                double output = outputExport.Value;
            };
            tran.Run(ckt);
            // </example_Transient>
        }

        [Test]
        public void When_ResistorModified_Expect_NoException()
        {
            // <example_Stochastic>
            // Build the circuit
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "0", 1.0e3));

            // Create the simulation
            OP op = new OP("Op 1");

            // Attach events to apply stochastic variation
            Random rndGenerator = new Random();
            int counter = 0;
            op.BeforeExecute += (sender, args) =>
            {
                // Apply a random value of 1kOhm with 5% tolerance
                double value = 950 + 100 * rndGenerator.NextDouble();
                Simulation sim = (Simulation)sender;
                sim.EntityBehaviors["R1"].GetParameterSet<SpiceSharp.Components.Resistors.Parameters>().Resistance = value;
            };
            op.AfterExecute += (sender, args) =>
            {
                // Run 10 times
                counter++;
                args.Repeat = counter < 10;
            };

            // Make the exports
            RealPropertyExport current = new RealPropertyExport(op, "R1", "i");

            // Simulate
            op.ExportSimulationData += (sender, args) =>
            {
                // This will run 1o times
                double result = current.Value;
            };
            op.Run(ckt);
            // </example_Stochastic>
        }

        [Test]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "Example code")]
        public void When_SimpleValidation_Expect_Reference()
        {
            // <example_Validation>
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new VoltageSource("V2", "in", "0", 2.0));
            IRules rules = ckt.Validate();
            if (rules.ViolationCount > 0)
            {
                // We have rules that were violated
                foreach (IRuleViolation violation in rules.Violations)
                {
                    // Handle rule violations
                }
            }
            // </example_Validation>
            Assert.AreEqual(1, rules.ViolationCount);
        }

        [Test]
        public void When_Documentation_Expect_NoException()
        {
            // <example_EntityDocumentation>
            ResistorModel entity = new ResistorModel("RM1");
            // using SpiceSharp.Reflection;
            foreach (MemberDocumentation parameter in entity.Parameters())
            {
                Console.Write(string.Join(", ", parameter.Names));
                Console.WriteLine($" : {parameter.Description} ({parameter.Member.Name}, {parameter.BaseType.Name})");
            }
            // </example_EntityDocumentation>

            Console.WriteLine();

            // <example_SimulationDocumentation>
            Transient simulation = new Transient("tran");
            // using SpiceSharp.Reflection;
            foreach (MemberDocumentation parameter in simulation.Parameters())
            {
                Console.Write(string.Join(", ", parameter.Names));
                Console.WriteLine($" : {parameter.Description} ({parameter.Member.Name}, {parameter.BaseType.Name})");
            }
            // </example_SimulationDocumentation>

            Console.WriteLine();

            // <example_BehaviorDocumentation>
            OP op = new OP("op");
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1),
                new Resistor("R1", "in", "0", 1e3));
            op.AfterSetup += (sender, args) =>
            {
                // Behaviors are created when executing a simulation,
                // so we need to register for the event to have access to them.
                // using SpiceSharp.Reflection;
                foreach (MemberDocumentation parameter in op.EntityBehaviors["V1"].Parameters())
                {
                    Console.Write(string.Join(", ", parameter.Names));
                    Console.WriteLine($" : {parameter.Description} ({parameter.Member.Name}, {parameter.BaseType.Name})");
                }
            };
            op.Run(ckt);
            // </example_BehaviorDocumentation>
        }
    }
}
