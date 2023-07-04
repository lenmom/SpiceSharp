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
    public class VoltageSwitchTests : Framework
    {
        private VoltageSwitch CreateVoltageSwitch(string name, string pos, string neg, string contPos, string contNeg, string model)
        {
            VoltageSwitch vsw = new VoltageSwitch(name, pos, neg, contPos, contNeg, model);
            return vsw;
        }

        private VoltageSwitchModel CreateVoltageSwitchModel(string name, string parameters)
        {
            VoltageSwitchModel model = new VoltageSwitchModel(name);
            ApplyParameters(model, parameters);
            return model;
        }

        [Test]
        public void When_SimpleSwitchDC_Expect_Spice3f5Reference()
        {
            // NOTE: The hysteresis is chosen such that it does not switch on the same point as a sweep. If that happens, then the smallest
            // numerical error can lead to a big output change, causing a mismatch between the reference.

            // Build the circuit
            Circuit ckt = new Circuit(
                CreateVoltageSwitch("S1", "out", "0", "in", "0", "myswitch"),
                CreateVoltageSwitchModel("myswitch", "VT=0.5 RON=1 ROFF=1e3 VH=0.2001"),
                new VoltageSource("V1", "in", "0", 0),
                new VoltageSource("V2", "vdd", "0", 5.0),
                new Resistor("R1", "vdd", "out", 1e3)
                );

            // Create the simulation, exports and references
            DC dc = new DC("DC", "V1", -3, 3, 10e-3);
            IExport<double>[] exports = { new RealVoltageExport(dc, "out") };
            double[][] references =
            {
                new[]
                {
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00,
                    2.500000000000000e+00, 2.500000000000000e+00, 2.500000000000000e+00, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03, 4.995004995004996e-03,
                    4.995004995004996e-03
                }
            };
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleSwitchSmallSignal_Expect_Reference()
        {
            Circuit ckt = new Circuit(
                new VoltageSource("V1", "in", "0", -1),
                CreateVoltageSwitch("S1", "out", "0", "in", "0", "myswitch"),
                new VoltageSource("Vdd", "vdd", "0", 5).SetParameter("acmag", 1.0),
                new Resistor("R1", "out", "vdd", 1e3),
                CreateVoltageSwitchModel("myswitch", "VT=0.5 RON=1 ROFF=1e3 VH=0.2001")
                );

            AC ac = new AC("ac", new DecadeSweep(1, 1e6, 2));
            IExport<Complex>[] exports = new IExport<Complex>[] { new ComplexVoltageExport(ac, "out") };
            Func<double, Complex>[] reference = new Func<double, Complex>[] { f => 0.5 };
            AnalyzeAC(ac, ckt, exports, reference);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleSwitchTransient_Expect_Spice3f5Reference()
        {
            // Build the switch
            Circuit ckt = new Circuit(
                CreateVoltageSwitch("S1", "0", "OUT", "IN", "0", "MYSW"),
                CreateVoltageSwitchModel("MYSW", "Ron=1 Roff=1e6 Vt=0.5 Vh=-0.4"),
                new VoltageSource("V1", "IN", "0", new Pulse(0, 1, 0.0, 0.4e-3, 0.4e-3, 0.1e-3, 1e-3)),
                new VoltageSource("V2", "N001", "0", 3.3),
                new Resistor("R1", "N001", "OUT", 1e3)
                );

            // Build simulation, exports and references
            Transient tran = new Transient("Tran 1", 0.1e-3, 3e-3);
            IExport<double>[] exports = { new GenericExport<double>(tran, () => tran.GetState<IIntegrationMethod>().Time), new RealVoltageExport(tran, "OUT") };
            double[][] references =
            {
                new[]
                {
                    0.000000000000000e+00, 6.000000000000000e-07, 1.200000000000000e-06, 2.400000000000000e-06,
                    4.800000000000000e-06, 9.600000000000000e-06, 1.920000000000000e-05, 3.840000000000000e-05,
                    7.680000000000000e-05, 1.368000000000000e-04, 1.968000000000000e-04, 2.568000000000000e-04,
                    3.168000000000000e-04, 3.768000000000000e-04, 4.000000000000000e-04, 4.060000000000000e-04,
                    4.180000000000000e-04, 4.420000000000000e-04, 4.900000000000000e-04, 5.000000000000000e-04,
                    5.060000000000000e-04, 5.180000000000000e-04, 5.420000000000001e-04, 5.900000000000000e-04,
                    6.500000000000001e-04, 7.100000000000001e-04, 7.700000000000002e-04, 8.300000000000002e-04,
                    8.900000000000003e-04, 9.000000000000000e-04, 9.060000000000000e-04, 9.180000000000000e-04,
                    9.420000000000000e-04, 9.900000000000000e-04, 1.000000000000000e-03, 1.006000000000000e-03,
                    1.018000000000000e-03, 1.042000000000000e-03, 1.090000000000000e-03, 1.150000000000000e-03,
                    1.210000000000000e-03, 1.270000000000000e-03, 1.330000000000000e-03, 1.390000000000000e-03,
                    1.400000000000000e-03, 1.406000000000000e-03, 1.418000000000000e-03, 1.442000000000000e-03,
                    1.490000000000000e-03, 1.500000000000000e-03, 1.506000000000000e-03, 1.518000000000000e-03,
                    1.542000000000000e-03, 1.590000000000000e-03, 1.650000000000000e-03, 1.710000000000000e-03,
                    1.770000000000000e-03, 1.830000000000000e-03, 1.890000000000000e-03, 1.900000000000000e-03,
                    1.906000000000000e-03, 1.918000000000000e-03, 1.942000000000000e-03, 1.990000000000000e-03,
                    2.000000000000000e-03, 2.006000000000000e-03, 2.018000000000000e-03, 2.042000000000000e-03,
                    2.090000000000000e-03, 2.150000000000000e-03, 2.210000000000000e-03, 2.270000000000000e-03,
                    2.330000000000000e-03, 2.390000000000001e-03, 2.400000000000000e-03, 2.406000000000000e-03,
                    2.418000000000000e-03, 2.442000000000000e-03, 2.490000000000000e-03, 2.500000000000000e-03,
                    2.506000000000000e-03, 2.518000000000000e-03, 2.542000000000000e-03, 2.590000000000000e-03,
                    2.650000000000000e-03, 2.710000000000000e-03, 2.770000000000000e-03, 2.830000000000000e-03,
                    2.890000000000001e-03, 2.900000000000000e-03, 2.906000000000000e-03, 2.918000000000000e-03,
                    2.942000000000000e-03, 2.990000000000000e-03, 3.000000000000000e-03
                },
                new[]
                {
                    3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00,
                    3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00,
                    3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00,
                    3.296703296703297e+00, 3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03,
                    3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03,
                    3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03,
                    3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03,
                    3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00,
                    3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00,
                    3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00,
                    3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703298e-03,
                    3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03,
                    3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03,
                    3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03,
                    3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703297e+00, 3.296703296703297e+00,
                    3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00,
                    3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00,
                    3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00,
                    3.296703296703297e+00, 3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03,
                    3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03,
                    3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03,
                    3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03, 3.296703296703298e-03,
                    3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00,
                    3.296703296703297e+00, 3.296703296703297e+00, 3.296703296703297e+00
                }
            };
            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_BooleanParameter_Expect_DirectAccess()
        {
            // Create voltage source
            VoltageSwitch s = new VoltageSwitch("SW 1");
            SpiceSharp.Components.Switches.Parameters p = s.Parameters;

            // Check on
            s.SetParameter("on", true);
            Assert.AreEqual(true, p.ZeroState);

            // Check off
            s.SetParameter("off", true);
            Assert.AreEqual(false, p.ZeroState);
        }

        [Test]
        public void When_OpenCircuitInput_Expect_SimulationValidationFailedException()
        {
            Circuit ckt = new Circuit(
                CreateVoltageSwitchModel("MYSW", "Ron=1 Roff=1e6 Vt=0.5 Vh=-0.4"),
                new VoltageSwitch("S1", "out", "0", "in", "0", "MYSW"));

            // Make the simulation and run it
            OP op = new OP("op");
            ValidationFailedException ex = Assert.Throws<ValidationFailedException>(() => op.Run(ckt));
            Assert.AreEqual(1, ex.Rules.ViolationCount);
            IRuleViolation violation = ex.Rules.Violations.First();
            Assert.IsInstanceOf<FloatingNodeRuleViolation>(violation);
            Assert.AreEqual("in", ((FloatingNodeRuleViolation)violation).FloatingVariable.Name);
        }

        [Test]
        public void When_ParallelMultiplier_Expect_Reference()
        {
            // Build the circuit
            Circuit ckt_ref = new Circuit(
                CreateVoltageSwitch("S1", "out", "0", "in", "0", "myswitch"),
                CreateVoltageSwitch("S2", "out", "0", "in", "0", "myswitch"),
                CreateVoltageSwitchModel("myswitch", "VT=0.5 RON=1 ROFF=1e3 VH=0.2001"),
                new VoltageSource("V1", "in", "0", 0),
                new VoltageSource("V2", "vdd", "0", 5.0),
                new Resistor("R1", "vdd", "out", 1e3)
                );
            Circuit ckt_act = new Circuit(
                CreateVoltageSwitch("S1", "out", "0", "in", "0", "myswitch")
                    .SetParameter("m", 2.0),
                CreateVoltageSwitchModel("myswitch", "VT=0.5 RON=1 ROFF=1e3 VH=0.2001"),
                new VoltageSource("V1", "in", "0", 0),
                new VoltageSource("V2", "vdd", "0", 5.0),
                new Resistor("R1", "vdd", "out", 1e3)
                );

            DC dc = new DC("dc", "V1", 0.0, 1.0, 0.1);
            RealVoltageExport[] exports = new[] { new RealVoltageExport(dc, "out") };
            Compare(dc, ckt_ref, ckt_act, exports);
            DestroyExports(exports);
        }

        [Test]
        public void When_ParallelMultiplierAC_Expect_Reference()
        {
            // Build the circuit
            Circuit ckt_ref = new Circuit(
                CreateVoltageSwitch("S1", "out", "0", "in", "0", "myswitch"),
                CreateVoltageSwitch("S2", "out", "0", "in", "0", "myswitch"),
                CreateVoltageSwitchModel("myswitch", "VT=0.5 RON=1 ROFF=1e3 VH=0.2001"),
                new VoltageSource("V1", "in", "0", 0),
                new VoltageSource("V2", "vdd", "0", 5.0),
                new Resistor("R1", "vdd", "out", 1e3)
                );
            Circuit ckt_act = new Circuit(
                CreateVoltageSwitch("S1", "out", "0", "in", "0", "myswitch")
                    .SetParameter("m", 2.0),
                CreateVoltageSwitchModel("myswitch", "VT=0.5 RON=1 ROFF=1e3 VH=0.2001"),
                new VoltageSource("V1", "in", "0", 0),
                new VoltageSource("V2", "vdd", "0", 5.0),
                new Resistor("R1", "vdd", "out", 1e3)
                );

            AC ac = new AC("ac", new DecadeSweep(1, 1e6, 2));
            ComplexVoltageExport[] exports = new[] { new ComplexVoltageExport(ac, "out") };
            Compare(ac, ckt_ref, ckt_act, exports);
            DestroyExports(exports);
        }
    }
}
