﻿using System;
using System.Collections.Generic;

using NUnit.Framework;

using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

using SpiceSharpTest.Models;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class DCTests : Framework
    {
        private Diode CreateDiode(string name, string anode, string cathode, string model)
        {
            Diode d = new Diode(name) { Model = model };
            d.Connect(anode, cathode);
            return d;
        }

        private DiodeModel CreateDiodeModel(string name, string parameters)
        {
            DiodeModel model = new DiodeModel(name);
            ApplyParameters(model, parameters);
            return model;
        }

        [Test]
        public void When_DCSweepResistorParameter_Expect_Reference()
        {
            // Create the circuit
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0),
                new Resistor("R1", "in", "out", 1.0e4),
                new Resistor("R2", "out", "0", 1.0e4)
                );

            // Do a DC sweep where one of the sweeps is a parameter
            DC dc = new DC("DC 1");
            dc.DCParameters.Sweeps.Add(new ParameterSweep("R2", "resistance", new LinearSweep(0.0, 1e4, 1e3), container =>
            {
                container.GetValue<ITemperatureBehavior>().Temperature();
            })); // Sweep R2 from 0 to 10k per 1k
            dc.DCParameters.Sweeps.Add(new ParameterSweep("V1", new LinearSweep(0, 5, 0.1))); // Sweep V1 from 0V to 5V per 100mV

            // Run simulation
            dc.ExportSimulationData += (sender, args) =>
            {
                double resistance = Math.Max(dc.GetCurrentSweepValue()[0], SpiceSharp.Components.Resistors.Parameters.MinimumResistance);
                double voltage = dc.GetCurrentSweepValue()[1];
                double expected = voltage * resistance / (resistance + 1.0e4);
                Assert.AreEqual(expected, args.GetVoltage("out"), 1e-12);
            };
            dc.Run(ckt);
        }

        [Test]
        public void When_DiodeDCTwice_Expect_NoException()
        {
            /*
             * Bug found by Marcin Golebiowski
             * Running simulations twice will give rise to errors. We are using a diode model here
             * in order to make sure we're use states, extra equations, etc.
             */

            Circuit ckt = new Circuit
            {
                CreateDiode("D1", "OUT", "0", "1N914"),
                CreateDiodeModel("1N914", "Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9"),
                new VoltageSource("V1", "OUT", "0", 0.0)
            };

            // Create simulations
            DC dc = new DC("DC 1", "V1", -1, 1, 10e-3);
            OP op = new OP("OP 1");

            // Create exports
            RealPropertyExport dcExportV1 = new RealPropertyExport(dc, "V1", "i");
            RealPropertyExport dcExportV12 = new RealPropertyExport(dc, "V1", "i");
            dc.ExportSimulationData += (sender, args) =>
            {
                double v1 = dcExportV1.Value;
                double v12 = dcExportV12.Value;
            };
            RealPropertyExport opExportV1 = new RealPropertyExport(op, "V1", "i");
            op.ExportSimulationData += (sender, args) =>
            {
                double v1 = opExportV1.Value;
            };

            // Run DC and op
            dc.Run(ckt);
            dc.Run(ckt);
        }

        [Test]
        public void When_DiodeDCRerun_Expect_Same()
        {
            Circuit ckt = new Circuit
            {
                CreateDiode("D1", "OUT", "0", "1N914"),
                CreateDiodeModel("1N914", "Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9"),
                new VoltageSource("V1", "OUT", "0", 0.0)
            };

            // Create simulations
            DC dc = new DC("DC 1", "V1", -1, 1, 10e-3);

            // Create exports
            RealPropertyExport dcExportV1 = new RealPropertyExport(dc, "V1", "i");

            // First run: build the reference
            List<double> r = new List<double>();
            void BuildReference(object sender, ExportDataEventArgs args)
            {
                r.Add(dcExportV1.Value);
            }

            dc.ExportSimulationData += BuildReference;
            dc.Run(ckt);
            dc.ExportSimulationData -= BuildReference;

            // Rerun: check with reference
            int index = 0;
            void CheckReference(object sender, ExportDataEventArgs args)
            {
                Assert.AreEqual(dcExportV1.Value, r[index++], 1e-20);
            }

            dc.ExportSimulationData += CheckReference;
            dc.Rerun();
            dc.ExportSimulationData -= CheckReference;
        }

        [Test]
        public void When_MultipleDC_Expect_Reference()
        {
            /*
             * We test if the simulation can run twice on different circuits with
             * different number of equations.
             */
            Circuit cktA = new Circuit(
                new VoltageSource("V1", "in", "0", 0),
                new Resistor("R1", "in", "out", 1e3),
                new Resistor("R2", "out", "0", 1e3));
            Circuit cktB = new Circuit(
                new VoltageSource("V1", "in", "0", 0),
                new Resistor("R1", "in", "out", 1e3),
                new Resistor("R2", "out", "int", 1e3),
                new Resistor("R3", "int", "0", 1e3));

            DC dc = new DC("dc", "V1", -2, 2, 0.1);
            bool a = true;
            dc.ExportSimulationData += (sender, args) =>
            {
                if (a)
                {
                    Assert.AreEqual(args.GetVoltage("in") * 0.5, args.GetVoltage("out"), 1e-12);
                }
                else
                {
                    Assert.AreEqual(args.GetVoltage("in") * 2.0 / 3.0, args.GetVoltage("out"), 1e-12);
                }
            };
            a = false; // Doing second circuit
            dc.Run(cktB);
            a = true; // Doing first circuit
            dc.Run(cktA);
        }
    }
}
