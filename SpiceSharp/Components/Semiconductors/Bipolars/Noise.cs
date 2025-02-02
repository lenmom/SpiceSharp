﻿using System;

using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Bipolars
{
    /// <summary>
    /// Noise behavior for <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    [BehaviorFor(typeof(BipolarJunctionTransistor)), AddBehaviorIfNo(typeof(INoiseBehavior))]
    public class Noise : Frequency, INoiseBehavior
    {
        private readonly INoiseSimulationState _noise;
        private readonly NoiseThermal _rc, _rb, _re;
        private readonly NoiseShot _ic, _ib;
        private readonly NoiseGain _flicker;

        /// <inheritdoc/>
        public double OutputNoiseDensity
        {
            get
            {
                return _rc.OutputNoiseDensity + _rb.OutputNoiseDensity + _re.OutputNoiseDensity +
            _ic.OutputNoiseDensity + _ib.OutputNoiseDensity + _flicker.OutputNoiseDensity;
            }
        }

        /// <inheritdoc/>
        public double TotalOutputNoise
        {
            get
            {
                return _rc.TotalOutputNoise + _rb.TotalOutputNoise + _re.TotalOutputNoise +
            _ic.TotalOutputNoise + _ib.TotalOutputNoise + _flicker.TotalOutputNoise;
            }
        }

        /// <inheritdoc/>
        public double TotalInputNoise
        {
            get
            {
                return _rc.TotalInputNoise + _rb.TotalInputNoise + _re.TotalInputNoise +
            _ic.TotalInputNoise + _ib.TotalInputNoise + _flicker.TotalInputNoise;
            }
        }

        /// <summary>
        /// Gets the thermal noise source of the resistor at the collector.
        /// </summary>
        /// <value>
        /// The thermal noise source.
        /// </value>
        [ParameterName("rc"), ParameterInfo("The thermal noise at the collector")]
        public INoiseSource ThermalCollectorResistor
        {
            get
            {
                return _rc;
            }
        }

        /// <summary>
        /// Gets the thermal noise source of the resistor at the base.
        /// </summary>
        /// <value>
        /// The thermal noise source.
        /// </value>
        [ParameterName("rb"), ParameterInfo("Ther thermal noise at the base")]
        public INoiseSource ThermalBaseResistor
        {
            get
            {
                return _rb;
            }
        }

        /// <summary>
        /// Gets the thermal noise source of the resistor at the emitter.
        /// </summary>
        /// <value>
        /// The thermal noise source.
        /// </value>
        public INoiseSource ThermalEmitterResistor
        {
            get
            {
                return _re;
            }
        }

        /// <summary>
        /// Gets the shot noise source of the collector-emitter current.
        /// </summary>
        /// <value>
        /// The shot noise source.
        /// </value>
        [ParameterName("ic"), ParameterInfo("The shot noise of the collector-emitter current")]
        public INoiseSource ShotCollectorCurrent
        {
            get
            {
                return _ic;
            }
        }

        /// <summary>
        /// Gets the shot noise of the base-emitter current.
        /// </summary>
        /// <value>
        /// The shot noise source.
        /// </value>
        [ParameterName("ib"), ParameterInfo("The shot noise of the base-emitter current")]
        public INoiseSource ShotBaseCurrent
        {
            get
            {
                return _ib;
            }
        }

        /// <summary>
        /// Gets the flicker noise source.
        /// </summary>
        /// <value>
        /// The flicker noise source.
        /// </value>
        [ParameterName("flicker"), ParameterInfo("The flicker noise")]
        public INoiseSource Flicker
        {
            get
            {
                return _flicker;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Noise(IComponentBindingContext context)
            : base(context)
        {
            IComplexSimulationState complex = context.GetState<IComplexSimulationState>();
            _noise = context.GetState<INoiseSimulationState>();

            IVariable<System.Numerics.Complex> c = complex.GetSharedVariable(context.Nodes[0]);
            IVariable<System.Numerics.Complex> b = complex.GetSharedVariable(context.Nodes[1]);
            IVariable<System.Numerics.Complex> e = complex.GetSharedVariable(context.Nodes[2]);
            IVariable<System.Numerics.Complex> bp = BasePrime;
            IVariable<System.Numerics.Complex> cp = CollectorPrime;
            IVariable<System.Numerics.Complex> ep = EmitterPrime;

            _rc = new NoiseThermal("rc", c, cp);
            _rb = new NoiseThermal("rb", b, bp);
            _re = new NoiseThermal("re", e, ep);
            _ic = new NoiseShot("ic", cp, ep);
            _ib = new NoiseShot("ib", bp, ep);
            _flicker = new NoiseGain("1/f", bp, ep);
        }

        /// <inheritdoc/>
        void INoiseSource.Initialize()
        {
            _rc.Initialize();
            _rb.Initialize();
            _re.Initialize();
            _ib.Initialize();
            _ic.Initialize();
            _flicker.Initialize();
        }

        /// <inheritdoc/>
        void INoiseBehavior.Compute()
        {
            // Compute thermal noises
            _rc.Compute(ModelTemperature.CollectorConduct * Parameters.Area, Parameters.Temperature);
            _rb.Compute(ConductanceX, Parameters.Temperature);
            _re.Compute(ModelTemperature.EmitterConduct * Parameters.Area, Parameters.Temperature);

            _ic.Compute(CollectorCurrent);
            _ib.Compute(BaseCurrent);
            _flicker.Compute(ModelParameters.FlickerNoiseCoefficient * Math.Exp(ModelParameters.FlickerNoiseExponent
                * Math.Log(Math.Max(Math.Abs(BaseCurrent), 1e-38))) / _noise.Point.Value.Frequency);
        }
    }
}
