﻿using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharpTest.Models
{
    /// <summary>
    /// From LTSpice
    /// .model 1N914 D(Is= 2.52n Rs = .568 N= 1.752 Cjo= 4p M = .4 tt= 20n Iave = 200m Vpk= 75 mfg= OnSemi type= silicon)
    /// </summary>
    [TestFixture]
    public class DiodeTests : Framework
    {
        private Diode CreateDiode(string name, string anode, string cathode, string model)
        {
            var d = new Diode(name, anode, cathode, model);
            return d;
        }

        private DiodeModel CreateDiodeModel(string name, string parameters)
        {
            var dm = new DiodeModel(name);
            ApplyParameters(dm, parameters);
            return dm;
        }

        [Test]
        public void When_GetPropertyOP_Expect_Reference()
        {
            // https://github.com/SpiceSharp/SpiceSharp/issues/169
            var dModel = new DiodeModel("test");
            var ckt = new Circuit(
                new VoltageSource("V1", "a", "0", 0),
                new Resistor("R1", "b", "0", 1000),
                new Diode("LED", "a", "b", "test"),
                dModel
                );

            var dc = new DC("DC", "V1", -3.0, 3.0, 0.1);

            var voltageExport = new RealPropertyExport(dc, "LED", "v");

            dc.ExportSimulationData += (sender, args) =>
            {
                var voltage = voltageExport.Value;
                var voltage2 = args.GetVoltage("a") - args.GetVoltage("b");

                // Because the property is always one iteration behind on the current solution, we relax the error a little bit
                var tol = Math.Max(Math.Abs(voltage), Math.Abs(voltage2)) * RelTol + 1e-9;
                Assert.AreEqual(voltage2, voltage, tol);
            };
            dc.Run(ckt);
        }

        [Test]
        public void When_SimpleDC_Expect_Spice3f5Reference()
        {
            /*
             * DC voltage shunted by a diode
             * Current is to behave like the reference
             */

            // Build circuit
            var ckt = new Circuit
            {
                CreateDiode("D1", "OUT", "0", "1N914"),
                CreateDiodeModel("1N914", "Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9"),
                new VoltageSource("V1", "OUT", "0", 0.0)
            };

            // Create simulation
            var dc = new DC("DC", "V1", -1.0, 1.0, 10e-3);

            // Create exports
            IExport<double>[] exports = { new RealPropertyExport(dc, "V1", "i") };

            // Create reference
            double[][] references =
            {
                new[] { 2.520684772022719e-09, 2.520665232097485e-09, 2.520645248083042e-09, 2.520624819979389e-09, 2.520603725741921e-09, 2.520582409459848e-09, 2.520560649088566e-09, 2.520538000538863e-09, 2.520515129944556e-09, 2.520491593216434e-09, 2.520467612399102e-09, 2.520442965447955e-09, 2.520417652362994e-09, 2.520391229055008e-09, 2.520364583702417e-09, 2.520336828126801e-09, 2.520307962328161e-09, 2.520278874484916e-09, 2.520248454374041e-09, 2.520216701995537e-09, 2.520184505527823e-09, 2.520150754747874e-09, 2.520115449655691e-09, 2.520079700474298e-09, 2.520041952891461e-09, 2.520002650996389e-09, 2.519962460922898e-09, 2.519919828358752e-09, 2.519875419437767e-09, 2.519829456204548e-09, 2.519781050480674e-09, 2.519730646355356e-09, 2.519677799739384e-09, 2.519621844498943e-09, 2.519563668812452e-09, 2.519502162456888e-09, 2.519437547476855e-09, 2.519369379783143e-09, 2.519297437331147e-09, 2.519221276031658e-09, 2.519140673840070e-09, 2.519055408711779e-09, 2.518964592468365e-09, 2.518868003065222e-09, 2.518765085390839e-09, 2.518655506378309e-09, 2.518538155804606e-09, 2.518412922647428e-09, 2.518278474639146e-09, 2.518133923601340e-09, 2.517978381355590e-09, 2.517810848701174e-09, 2.517629882348160e-09, 2.517434039006616e-09, 2.517221764364308e-09, 2.516991060019791e-09, 2.516739705527016e-09, 2.516465702484538e-09, 2.516165609200982e-09, 2.515836872163391e-09, 2.515475161501968e-09, 2.515076480413825e-09, 2.514635832895351e-09, 2.514147445786818e-09, 2.513604324683172e-09, 2.512998475978634e-09, 2.512320573799798e-09, 2.511559182849510e-09, 2.510700980451475e-09, 2.509729757349533e-09, 2.508625973618450e-09, 2.507366425597013e-09, 2.505921525841615e-09, 2.504256357838130e-09, 2.502326790221332e-09, 2.500077533884593e-09, 2.497439421933478e-09, 2.494324247148683e-09, 2.490618655759391e-09, 2.486175321170236e-09, 2.480800564974572e-09, 2.474236482363779e-09, 2.466134130241215e-09, 2.456014613905211e-09, 2.443208080293857e-09, 2.426758793916406e-09, 2.405272869765440e-09, 2.377086694149710e-09, 2.341755483969976e-09, 2.297702500486665e-09, 2.242774105321033e-09, 2.174284835509965e-09, 2.088886258411193e-09, 1.982402894618041e-09, 1.849628367134315e-09, 1.684070757845824e-09, 1.477634958835239e-09, 1.220227058285062e-09, 8.992606936875092e-10, 4.990415580774510e-10, -4.208324063460023e-23, -6.222658915921997e-10, -1.398183520351370e-09, -2.365693620165477e-09, -3.572105541915782e-09, -5.076410555804323e-09, -6.952166481388744e-09, -9.291094477115180e-09, -1.220756418174318e-08, -1.584418615752092e-08, -2.037878504834723e-08, -2.603309548487864e-08, -3.308360396747645e-08, -4.187506874586688e-08, -5.283737797290300e-08, -6.650657008444583e-08, -8.355104497148602e-08, -1.048042475026989e-07, -1.313054202034536e-07, -1.643504193848955e-07, -2.055550786805860e-07, -2.569342167357824e-07, -3.210001533471285e-07, -4.008855497006358e-07, -5.004965768495850e-07, -6.247039000539800e-07, -7.795808144028804e-07, -9.727001679671332e-07, -1.213504582209257e-06, -1.513768057126441e-06, -1.888171507258285e-06, -2.355020333966173e-06, -2.937139061076621e-06, -3.662986687191783e-06, -4.568047149322574e-06, -5.696562662471649e-06, -7.103694343424394e-06, -8.858215224893939e-06, -1.104586649613992e-05, -1.377353976839135e-05, -1.717448782878606e-05, -2.141481549700064e-05, -2.670156304629412e-05, -3.329276978536466e-05, -4.150999799246158e-05, -5.175391113643180e-05, -6.452363948283857e-05, -8.044083562419591e-05, -1.002795274224200e-04, -1.250031216609715e-04, -1.558102032684916e-04, -1.941911156088105e-04, -2.419976971548277e-04, -3.015289829039203e-04, -3.756361388815854e-04, -4.678503519079946e-04, -5.825377853404534e-04, -7.250859365310891e-04, -9.021256392514054e-04, -1.121792314356274e-03, -1.394028558000970e-03, -1.730927332259435e-03, -2.147110349238757e-03, -2.660129130047428e-03, -3.290866177790397e-03, -4.063900589218683e-03, -5.007786885759424e-03, -6.155179858715831e-03, -7.542725667060601e-03, -9.210636215301937e-03, -1.120187715360643e-02, -1.356093587232876e-02, -1.633219609264214e-02, -1.955802291413677e-02, -2.327673904312388e-02, -2.752072521737903e-02, -3.231488373640667e-02, -3.767565498752745e-02, -4.361068391378264e-02, -5.011912351695047e-02, -5.719246701131020e-02, -6.481574162201031e-02, -7.296888192813844e-02, -8.162812138207687e-02, -9.076728201979800e-02, -1.003588891563969e-01, -1.103750792457605e-01, -1.207882998568939e-01, -1.315718201008491e-01, -1.427000796200868e-01, -1.541489071231517e-01, -1.658956380366401e-01, -1.779191572005734e-01, -1.901998880621483e-01, -2.027197453741645e-01, -2.154620644191900e-01, -2.284115164341036e-01, -2.415540172223232e-01, -2.548766338536659e-01, -2.683674927728656e-01, -2.820156914701786e-01 }
            };

            // Run test
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleSmallSignal_Expect_Spice3f5Reference()
        {
            /*
             * DC voltage source shunted by a diode
             * Current is expected to behave like the reference
             */
            // Build circuit
            var ckt = new Circuit
            {
                CreateDiode("D1", "0", "OUT", "1N914"),
                CreateDiodeModel("1N914", "Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9"),
                new VoltageSource("V1", "OUT", "0", 1.0)
                    .SetParameter("acmag", 1.0)
            };

            // Create simulation
            var ac = new AC("ac", new DecadeSweep(1e3, 10e6, 5));

            // Create exports
            IExport<Complex>[] exports = { new ComplexPropertyExport(ac, "V1", "i") };

            // Create references
            double[] riRef = { -1.945791742986885e-12, -1.904705637099517e-08, -1.946103289747125e-12, -3.018754997881332e-08, -1.946885859826953e-12, -4.784404245850086e-08, -1.948851586992178e-12, -7.582769719229839e-08, -1.953789270386556e-12, -1.201788010800761e-07, -1.966192170307985e-12, -1.904705637099495e-07, -1.997346846331992e-12, -3.018754997881245e-07, -2.075603854314768e-12, -4.784404245849736e-07, -2.272176570837208e-12, -7.582769719228451e-07, -2.765944910274710e-12, -1.201788010800207e-06, -4.006234902415568e-12, -1.904705637097290e-06, -7.121702504803603e-12, -3.018754997872460e-06, -1.494740330300116e-11, -4.784404245814758e-06, -3.460467495474045e-11, -7.582769719089195e-06, -8.398150889530617e-11, -1.201788010744768e-05, -2.080105080892987e-10, -1.904705636876583e-05, -5.195572682013223e-10, -3.018754996993812e-05, -1.302127347221150e-09, -4.784404242316795e-05, -3.267854507347871e-09, -7.582769705163549e-05, -8.205537869558709e-09, -1.201788005200868e-04, -2.060843758802494e-08, -1.904705614805916e-04 };
            var references = new Complex[1][];
            references[0] = new Complex[riRef.Length / 2];
            for (var i = 0; i < riRef.Length; i += 2)
            {
                references[0][i / 2] = new Complex(riRef[i], riRef[i + 1]);
            }

            // Run test
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_CdSmallSignal_Expect_NoException()
        {
            // Bug reported by William C. Donaldson (dsonbill)
            // Issue 115
            var ckt = new Circuit
            {
                CreateDiode("D1", "0", "OUT", "1N914"),
                CreateDiodeModel("1N914", "Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9"),
                new VoltageSource("V1", "OUT", "0", 1.0)
                    .SetParameter("acmag", 1.0)
            };

            // Create simulation
            var ac = new AC("ac", new DecadeSweep(1e3, 10e6, 5));

            // Get the property Cd
            var export = new RealPropertyExport(ac, "D1", "cd");

            ac.Run(ckt);
        }

        [Test]
        public void When_RectifierTransient_Expect_Spice3f5Reference()
        {
            /*
             * Pulsed voltage source towards a resistive voltage divider between 0V and 5V
             * Output voltage is expected to behavior like the reference
             */
            // Build circuit
            var ckt = new Circuit
            {
                new VoltageSource("V1", "in", "0", new Pulse(0, 5, 1e-6, 10e-9, 10e-9, 1e-6, 2e-6)),
                new VoltageSource("Vsupply", "vdd", "0", 5.0),
                new Resistor("R1", "vdd", "out", 10.0e3),
                new Resistor("R2", "out", "0", 10.0e3),
                CreateDiode("D1", "in", "out", "1N914"),
                CreateDiodeModel("1N914", "Is = 2.52e-9 Rs = 0.568 N = 1.752 Cjo = 4e-12 M = 0.4 tt = 20e-9")
            };

            // Create simulation
            var tran = new Transient("tran", 1e-9, 10e-6);

            // Create exports
            IExport<double>[] exports = { new RealVoltageExport(tran, "out") };

            // Create references
            double[][] references =
            {
                new[] { 2.499987387600927e+00, 2.499987387600927e+00, 2.499987387600927e+00, 2.499987387600931e+00, 2.499987387600930e+00, 2.499987387600930e+00, 2.499987387600929e+00, 2.499987387600930e+00, 2.499987387600933e+00, 2.499987387600932e+00, 2.499987387600934e+00, 2.499987387600934e+00, 2.499987387600928e+00, 2.499987387600928e+00, 2.499987387600927e+00, 2.499987387600927e+00, 2.499987387600926e+00, 2.499987387600928e+00, 2.499987387600927e+00, 2.499987387600927e+00, 2.499987387600927e+00, 2.961897877874934e+00, 3.816739529558831e+00, 5.191526587205253e+00, 6.033962809915079e+00, 5.858560892037079e+00, 5.542221840643947e+00, 5.047948278928924e+00, 4.566221446174492e+00, 4.496222403679634e+00, 4.470638440255675e+00, 4.461747124949804e+00, 4.458828418109562e+00, 4.458043432344522e+00, 4.458104168570753e+00, 4.458101940017489e+00, 4.458103007267981e+00, 4.458102313327546e+00, 4.458102774372732e+00, 4.458102484881135e+00, 3.958696587137276e+00, 2.960963748330885e+00, 9.726613059613628e-01, -5.081214667849954e-01, -5.008294438061146e-01, -4.823457266305946e-01, -4.093596340082378e-01, 2.927533877100476e-01, 1.580990201373001e+00, 2.297731671229221e+00, 2.465955512323740e+00, 2.496576390164448e+00, 2.500278606332533e+00, 2.499872222320282e+00, 2.500061542030393e+00, 2.499929263366373e+00, 2.500032946810125e+00, 2.499951677124709e+00, 2.500009659840817e+00, 2.961918369398067e+00, 3.816756624765787e+00, 5.191537959402029e+00, 6.033971024560395e+00, 5.858568428923525e+00, 5.542228175717445e+00, 5.047952819852995e+00, 4.566222065264992e+00, 4.496222553506734e+00, 4.470638537994252e+00, 4.461747150642569e+00, 4.458828425715773e+00, 4.458043431871857e+00, 4.458104168387089e+00, 4.458101940091086e+00, 4.458103007220202e+00, 4.458102313359292e+00, 4.458102774351643e+00, 4.458102484892605e+00, 3.958696587148540e+00, 2.960963748341582e+00, 9.726613059724690e-01, -5.081214667705706e-01, -5.008294437900119e-01, -4.823457266089376e-01, -4.093596339411046e-01, 2.927533879025108e-01, 1.580990201486391e+00, 2.297731671249843e+00, 2.465955512329101e+00, 2.496576390165203e+00, 2.500278606332566e+00, 2.499872222320265e+00, 2.500061542030405e+00, 2.499929263366364e+00, 2.500032946810133e+00, 2.499951677124703e+00, 2.500009659840779e+00, 2.961918369398032e+00, 3.816756624765757e+00, 5.191537959401995e+00, 6.033971024560496e+00, 5.858568428923665e+00, 5.542228175717561e+00, 5.047952819853082e+00, 4.566222065265002e+00, 4.496222553506737e+00, 4.470638537994254e+00, 4.461747150642569e+00, 4.458828425715771e+00, 4.458043431871858e+00, 4.458104168387090e+00, 4.458101940091087e+00, 4.458103007220201e+00, 4.458102313359291e+00, 4.458102774351643e+00, 4.458102484892708e+00, 3.958696587148749e+00, 2.960963748341791e+00, 9.726613059726750e-01, -5.081214667704262e-01, -5.008294437900225e-01, -4.823457266089518e-01, -4.093596339411494e-01, 2.927533879023823e-01, 1.580990201486315e+00, 2.297731671249816e+00, 2.465955512329098e+00, 2.496576390165200e+00, 2.500278606332570e+00, 2.499872222320263e+00, 2.500061542030407e+00, 2.499929263366362e+00, 2.500032946810134e+00, 2.499951677124702e+00, 2.500009659840780e+00, 2.961918369398029e+00, 3.816756624765742e+00, 5.191537959401949e+00, 6.033971024560461e+00, 5.858568428923622e+00, 5.542228175717498e+00, 5.047952819852998e+00, 4.566222065264983e+00, 4.496222553506735e+00, 4.470638537994251e+00, 4.461747150642568e+00, 4.458828425715771e+00, 4.458043431871856e+00, 4.458104168387091e+00, 4.458101940091087e+00, 4.458103007220201e+00, 4.458102313359291e+00, 4.458102774351643e+00, 4.458102484892708e+00, 3.958696587148324e+00, 2.960963748341793e+00, 9.726613059726769e-01, -5.081214667704226e-01, -5.008294437900186e-01, -4.823457266089463e-01, -4.093596339411322e-01, 2.927533879024313e-01, 1.580990201486346e+00, 2.297731671249819e+00, 2.465955512329098e+00, 2.496576390165201e+00, 2.500278606332567e+00, 2.499872222320263e+00, 2.500061542030407e+00, 2.499929263366362e+00, 2.500032946810135e+00, 2.499951677124702e+00, 2.500009659840780e+00, 2.961918369398421e+00, 3.816756624765698e+00, 5.191537959401959e+00, 6.033971024560470e+00, 5.858568428923640e+00, 5.542228175717541e+00, 5.047952819853064e+00, 4.566222065265000e+00, 4.496222553506738e+00, 4.470638537994254e+00, 4.461747150642569e+00, 4.458828425715772e+00, 4.458043431871857e+00, 4.458104168387089e+00, 4.458101940091087e+00, 4.458103007220202e+00, 4.458102313359291e+00, 4.458102774351643e+00, 4.458102489285038e+00 }
            };

            // Run test
            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SimpleNoise_Expect_Spice3f5Reference()
        {
            // Build the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "out", 10e3),
                CreateDiode("D1", "out", "0", "1N914"),
                CreateDiodeModel("1N914", "Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9 Kf=1e-14 Af=0.9")
            );

            // Create the noise, exports and reference values
            var noise = new Noise("Noise", "V1", "out", new DecadeSweep(10, 10e9, 10));
            IExport<double>[] exports = { new InputNoiseDensityExport(noise), new OutputNoiseDensityExport(noise) };
            double[][] references =
            {
                new[]
                {
                    1.458723146141516e-11, 1.158744449455564e-11, 9.204629008621218e-12, 7.311891390005248e-12,
                    5.808436458613777e-12, 4.614199756974080e-12, 3.665583825917671e-12, 2.912071407970300e-12,
                    2.313535219179340e-12, 1.838101024918420e-12, 1.460450220663580e-12, 1.160471523977629e-12,
                    9.221899753841859e-13, 7.329162135225890e-13, 5.825707203834432e-13, 4.631470502194742e-13,
                    3.682854571138349e-13, 2.929342153191007e-13, 2.330805964400094e-13, 1.855371770139243e-13,
                    1.477720965884518e-13, 1.177742269198749e-13, 9.394607206055956e-14, 7.501869587444578e-14,
                    5.998414656060420e-14, 4.804177954432334e-14, 3.855562023394363e-14, 3.102049605476259e-14,
                    2.503513416731735e-14, 2.028079222544473e-14, 1.650428418406463e-14, 1.350449721905778e-14,
                    1.112168173606098e-14, 9.228944122102516e-15, 7.725489198094821e-15, 6.531252508160282e-15,
                    5.582636595658667e-15, 4.829124207122907e-15, 4.230588064951540e-15, 3.755153944584402e-15,
                    3.377503257451775e-15, 3.077524746402662e-15, 2.839243492037182e-15, 2.649970196512345e-15,
                    2.499625442488393e-15, 2.380202943769798e-15, 2.285343207313612e-15, 2.209994908152325e-15,
                    2.150145953086284e-15, 2.102609925372879e-15, 2.064856560106980e-15, 2.034877257821301e-15,
                    2.011078530414255e-15, 1.992197793764384e-15, 1.977237163343344e-15, 1.965411950136735e-15,
                    1.956111467419073e-15, 1.948870621182985e-15, 1.943351658816035e-15, 1.939336510665159e-15,
                    1.936731545908949e-15, 1.935588529057677e-15, 1.936148492385046e-15, 1.938919734396916e-15,
                    1.944808160599577e-15, 1.955329191908373e-15, 1.972947835070999e-15, 2.001620965174601e-15,
                    2.047659303373011e-15, 2.121095285435582e-15, 2.237851669884242e-15, 2.423177364004633e-15,
                    2.717087559609432e-15, 3.182970682239311e-15, 3.921190189710068e-15, 5.090542302029580e-15,
                    6.942013717266990e-15, 9.871657130196278e-15, 1.450283032947425e-14, 2.181265815258579e-14,
                    3.332290820407716e-14, 5.137924773110117e-14, 7.953823062476673e-14, 1.230515662817079e-13,
                    1.893480090868467e-13, 2.882127220525774e-13, 4.310267857547614e-13, 6.281208435935691e-13,
                    8.836523208957566e-13, 1.189393904473825e-12, 1.521954697611584e-12
                },
                new[]
                {
                    8.534638344391308e-14, 6.779535126890105e-14, 5.385407086373468e-14, 4.278011820970267e-14,
                    3.398376494660514e-14, 2.699657318711771e-14, 2.144644949112401e-14, 1.703782953318393e-14,
                    1.353593822442315e-14, 1.075428708293890e-14, 8.544743042104929e-15, 6.789639824603726e-15,
                    5.395511784087090e-15, 4.288116518683889e-15, 3.408481192374137e-15, 2.709762016425394e-15,
                    2.154749646826024e-15, 1.713887651032016e-15, 1.363698520155939e-15, 1.085533406007514e-15,
                    8.645790019241169e-16, 6.890686801739967e-16, 5.496558761223330e-16, 4.389163495820129e-16,
                    3.509528169510376e-16, 2.810808993561634e-16, 2.255796623962265e-16, 1.814934628168257e-16,
                    1.464745497292180e-16, 1.186580383143755e-16, 9.656259790603582e-17, 7.901156573102381e-17,
                    6.507028532585746e-17, 5.399633267182545e-17, 4.519997940872793e-17, 3.821278764924051e-17,
                    3.266266395324682e-17, 2.825404399530675e-17, 2.475215268654597e-17, 2.197050154506173e-17,
                    1.976095750422776e-17, 1.800585428672656e-17, 1.661172624620993e-17, 1.550433098080673e-17,
                    1.462469565449698e-17, 1.392597647854824e-17, 1.337096410894887e-17, 1.293010211315486e-17,
                    1.257991298227878e-17, 1.230174786813036e-17, 1.208079346404696e-17, 1.190528314229684e-17,
                    1.176587033824518e-17, 1.165513081170486e-17, 1.156716727907389e-17, 1.149729536147901e-17,
                    1.144179412451907e-17, 1.139770792493967e-17, 1.136268901185206e-17, 1.133487250043722e-17,
                    1.131277706002888e-17, 1.129522602785387e-17, 1.128128474744870e-17, 1.127021079479467e-17,
                    1.126141444153157e-17, 1.125442724977209e-17, 1.124887712607609e-17, 1.124446850611815e-17,
                    1.124096661480939e-17, 1.123818496366791e-17, 1.123597541962708e-17, 1.123422031640957e-17,
                    1.123282618836906e-17, 1.123171879310365e-17, 1.123083915777734e-17, 1.123014043860140e-17,
                    1.122958542623180e-17, 1.122914456423600e-17, 1.122879437510513e-17, 1.122851620999098e-17,
                    1.122829525558689e-17, 1.122811974526514e-17, 1.122798033246109e-17, 1.122786959293455e-17,
                    1.122778162940192e-17, 1.122771175748433e-17, 1.122765625624737e-17, 1.122761217004779e-17,
                    1.122757715113470e-17, 1.122754933462328e-17, 1.122752723918288e-17
                }
            };
            AnalyzeNoise(noise, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_MultipliersDC_Expect_Reference()
        {
            DiodeModel model = CreateDiodeModel("1N914", "Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9");
            var cktReference = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0), model);
            ParallelSeries(cktReference, name => new Diode(name, "", "", model.Name), "in", "0", 3, 2);
            var cktActual = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0), model,
                new Diode("D1", "in", "0", model.Name).SetParameter("m", 3.0).SetParameter("n", 2.0));

            var dc = new DC("dc", "V1", -1, 1, 0.1);
            dc.BiasingParameters.Gmin = 0.0; // May interfere with comparison
            var exports = new IExport<double>[] { new RealCurrentExport(dc, "V1") };

            Compare(dc, cktReference, cktActual, exports);
            DestroyExports(exports);
        }

        [Test]
        public void When_MultipliersSmallSignal_Expect_Reference()
        {
            DiodeModel model = CreateDiodeModel("1N914", "Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9");
            var cktReference = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0).SetParameter("acmag", 1.0), model);
            ParallelSeries(cktReference, name => new Diode(name, "", "", model.Name), "in", "0", 3, 2);
            var cktActual = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0).SetParameter("acmag", 1.0), model,
                new Diode("D1", "in", "0", model.Name).SetParameter("m", 3.0).SetParameter("n", 2.0));

            var ac = new AC("ac", new DecadeSweep(0.1, 1e6, 5));
            ac.BiasingParameters.Gmin = 0.0; // May interfere with comparison
            var exports = new IExport<Complex>[] { new ComplexCurrentExport(ac, "V1") };

            Compare(ac, cktReference, cktActual, exports);
            DestroyExports(exports);
        }

        [Test]
        public void When_MultipliersNoise_Expect_Reference()
        {
            DiodeModel model = CreateDiodeModel("1N914", "Is=2.52e-9 Rs=5680 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9 Kf=1e-10 Af=0.9");
            var cktReference = new Circuit(
                new VoltageSource("V1", "in", "0", 3),
                new Resistor("R1", "in", "out", 10e3),
                model);
            ParallelSeries(cktReference, name => new Diode(name, "", "", model.Name), "out", "0", 3, 2);
            var cktActual = new Circuit(
                new VoltageSource("V1", "in", "0", 3).SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "out", 10e3), model,
                new Diode("D1", "out", "0", model.Name).SetParameter("m", 3.0).SetParameter("n", 2.0));

            var noise = new Noise("noise", "V1", "out", new DecadeSweep(0.1, 1e6, 5));
            noise.BiasingParameters.Gmin = 0.0; // May interfere with comparison
            var exports = new IExport<double>[] { new InputNoiseDensityExport(noise), new OutputNoiseDensityExport(noise) };

            Compare(noise, cktReference, cktActual, exports);
            DestroyExports(exports);
        }

        [Test]
        public void When_MultipliersTransient_Expect_Reference()
        {
            /*
             * Pulsed voltage source towards a resistive voltage divider between 0V and 5V
             * Output voltage is expected to behavior like the reference
             */
            // Build circuit
            DiodeModel model = CreateDiodeModel("1N914", "Is = 2.52e-9 Rs = 0.568 N = 1.752 Cjo = 4e-12 M = 0.4 tt = 20e-9");
            var ckt = new Circuit(
                new VoltageSource("V1r", "inr", "0", new Pulse(0, 5, 1e-6, 10e-9, 10e-9, 1e-6, 2e-6)),
                new VoltageSource("Vsupplyr", "vddr", "0", 5.0),
                new Resistor("R1r", "vddr", "outr", 10.0e3),
                new Resistor("R2r", "outr", "0", 10.0e3),
                new VoltageSource("V1a", "ina", "0", new Pulse(0, 5, 1e-6, 10e-9, 10e-9, 1e-6, 2e-6)),
                new VoltageSource("Vsupplya", "vdda", "0", 5.0),
                new Resistor("R1a", "vdda", "outa", 10.0e3),
                new Resistor("R2a", "outa", "0", 10.0e3),
                model,
                new Diode("D1", "ina", "outa", model.Name)
                    .SetParameter("m", 3.0)
                    .SetParameter("n", 2.0));
            ParallelSeries(ckt, name => new Diode(name, "", "", model.Name), "inr", "outr", 3, 2);

            // Create simulation
            var tran = new Transient("tran", 1e-9, 10e-6);
            tran.BiasingParameters.Gmin = 0.0; // May interfere with comparison
            var v_ref = new RealVoltageExport(tran, "outr");
            var v_act = new RealVoltageExport(tran, "outa");
            tran.ExportSimulationData += (sender, args) =>
            {
                var tol = Math.Max(Math.Abs(v_ref.Value), Math.Abs(v_act.Value)) * CompareRelTol + CompareAbsTol;
                Assert.AreEqual(v_ref.Value, v_act.Value, tol);
            };
            tran.Run(ckt);
            v_ref.Destroy();
            v_act.Destroy();
        }

        [Test]
        public void When_Breakdown_Expect_Reference()
        {
            // Source: https://www.el-component.com/diodes/1n4148
            DiodeModel model = CreateDiodeModel("1N4148", "IS=4.352E-9 N=1.906 BV=110 IBV=0.0001 RS=0.6458 CJO=7.048E-13 VJ=0.869 M=0.03 FC=0.5 TT=3.48E-9");
            var ckt = new Circuit(
                new VoltageSource("V1", "a", "0", 0.0),
                CreateDiode("D1", "a", "0", "1N4148"),
                model);

            // Sweep
            var dc = new DC("dc", "V1", -111.0, 1.0, 0.5);
            RealCurrentExport[] exports = new[] { new RealCurrentExport(dc, "V1") };
            var references = new[]
            {
                new[]
                {
                    0.522401721734752, 0.0157009009533624, 7.59356510116049E-07, 4.46149783783767E-09, 4.46101466877735E-09, 4.4605030780076E-09,
                    4.45999148723786E-09, 4.45950831817754E-09, 4.45902514911722E-09, 4.45851355834748E-09, 4.45800196757773E-09, 4.45749037680798E-09,
                    4.45700720774767E-09, 4.45649561697792E-09, 4.4560124479176E-09, 4.45550085714785E-09, 4.45498926637811E-09, 4.45450609731779E-09,
                    4.45399450654804E-09, 4.45351133748773E-09, 4.45299974671798E-09, 4.45248815594823E-09, 4.45200498688791E-09, 4.4515218178276E-09,
                    4.45101022705785E-09, 4.45047021457867E-09, 4.45001546722779E-09, 4.44950387645804E-09, 4.44899228568829E-09, 4.44850911662797E-09,
                    4.44802594756766E-09, 4.44751435679791E-09, 4.44700276602816E-09, 4.44651959696785E-09, 4.4460080061981E-09, 4.44552483713778E-09,
                    4.4449848246586E-09, 4.44450165559829E-09, 4.44399006482854E-09, 4.44350689576822E-09, 4.44302372670791E-09, 4.44251213593816E-09,
                    4.44200054516841E-09, 4.44148895439866E-09, 4.44100578533835E-09, 4.44052261627803E-09, 4.43998260379885E-09, 4.43949943473854E-09,
                    4.43901626567822E-09, 4.43850467490847E-09, 4.43802150584816E-09, 4.43750991507841E-09, 4.43699832430866E-09, 4.43651515524834E-09,
                    4.4360035644786E-09, 4.43552039541828E-09, 4.4349803829391E-09, 4.43449721387879E-09, 4.43401404481847E-09, 4.43350245404872E-09,
                    4.43300507413369E-09, 4.43249348336394E-09, 4.43201031430362E-09, 4.43149872353388E-09, 4.43100134361885E-09, 4.4304897528491E-09,
                    4.43000658378878E-09, 4.42950920387375E-09, 4.428997613104E-09, 4.42851444404369E-09, 4.42800285327394E-09, 4.42750547335891E-09,
                    4.42700809344387E-09, 4.42651071352884E-09, 4.42599912275909E-09, 4.42548753198935E-09, 4.42501857378375E-09, 4.42452119386871E-09,
                    4.42399539224425E-09, 4.42349801232922E-09, 4.4230148432689E-09, 4.42251746335387E-09, 4.42200587258412E-09, 4.42149428181438E-09,
                    4.42101111275406E-09, 4.42051373283903E-09, 4.42000214206928E-09, 4.41950476215425E-09, 4.41900738223922E-09, 4.41849579146947E-09,
                    4.41799841155444E-09, 4.41751524249412E-09, 4.41700365172437E-09, 4.41649206095462E-09, 4.41599468103959E-09, 4.41549730112456E-09,
                    4.41499992120953E-09, 4.41451675214921E-09, 4.41399095052475E-09, 4.41350778146443E-09, 4.41299619069468E-09, 4.41249881077965E-09,
                    4.41200143086462E-09, 4.41150405094959E-09, 4.41100667103456E-09, 4.41050929111952E-09, 4.40999770034978E-09, 4.40950032043474E-09,
                    4.40901715137443E-09, 4.40850556060468E-09, 4.40800818068965E-09, 4.4074965899199E-09, 4.40699921000487E-09, 4.40651604094455E-09,
                    4.40600445017481E-09, 4.40550707025977E-09, 4.40500969034474E-09, 4.40449809957499E-09, 4.40400071965996E-09, 4.40350333974493E-09,
                    4.4030059598299E-09, 4.40249436906015E-09, 4.40201119999983E-09, 4.40149960923009E-09, 4.40101644016977E-09, 4.40050484940002E-09,
                    4.40000746948499E-09, 4.39949587871524E-09, 4.39899849880021E-09, 4.39850111888518E-09, 4.39800373897015E-09, 4.39750635905511E-09,
                    4.39699476828537E-09, 4.39651159922505E-09, 4.3960000084553E-09, 4.39551683939499E-09, 4.39499103777052E-09, 4.39449365785549E-09,
                    4.39399627794046E-09, 4.39349889802543E-09, 4.39300862353775E-09, 4.39250413819536E-09, 4.39200675828033E-09, 4.39150227293794E-09,
                    4.39100489302291E-09, 4.39050040768052E-09, 4.39000302776549E-09, 4.38950564785046E-09, 4.38900826793542E-09, 4.38851088802039E-09,
                    4.388006402678E-09, 4.38750902276297E-09, 4.38699743199322E-09, 4.38650715750555E-09, 4.38600267216316E-09, 4.38550529224813E-09,
                    4.38500080690574E-09, 4.3845034269907E-09, 4.38399894164831E-09, 4.38349445630593E-09, 4.38299707639089E-09, 4.38249969647586E-09,
                    4.38199521113347E-09, 4.3815049366458E-09, 4.38100045130341E-09, 4.38050307138838E-09, 4.37999858604599E-09, 4.3794941007036E-09,
                    4.37899672078856E-09, 4.37849223544617E-09, 4.37799485553114E-09, 4.37749747561611E-09, 4.37700009570108E-09, 4.37650271578605E-09,
                    4.37599823044366E-09, 4.37550085052862E-09, 4.37500347061359E-09, 4.3744989852712E-09, 4.37399449992881E-09, 4.37349712001378E-09,
                    4.37299263467139E-09, 4.37250236018372E-09, 4.37199787484133E-09, 4.37149694221262E-09, 4.37099956229758E-09, 4.37049507695519E-09,
                    4.36999769704016E-09, 4.36949676441145E-09, 4.36899938449642E-09, 4.36849845186771E-09, 4.36800107195268E-09, 4.36750013932397E-09,
                    4.36699920669525E-09, 4.36650182678022E-09, 4.36600089415151E-09, 4.3654999615228E-09, 4.36499902889409E-09, 4.36449809626538E-09,
                    4.36399716363667E-09, 4.36349623100796E-09, 4.36299529837925E-09, 4.36249436575054E-09, 4.36200053854918E-09, 4.36149960592047E-09,
                    4.3610004496486E-09, 4.36049596430621E-09, 4.36000036074802E-09, 4.35949765176247E-09, 4.3589984954906E-09, 4.35849756286188E-09,
                    4.35799840659001E-09, 4.35749569760446E-09, 4.35699565315417E-09, 4.35649383234704E-09, 4.35598845882623E-09, 4.35548397348384E-09,
                    4.35497415907093E-09, 4.35445457469541E-09, 4.35391234177018E-09, 4.35329239323323E-09, 4.3522991877154E-09, 4.34689306771929E-09,
                    -3.14063441399642E-22, -0.000110384655194973, -0.2012126027243
                }
            };
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }
    }
}
