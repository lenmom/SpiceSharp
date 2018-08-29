﻿using System;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Frequency-domain analysis (AC analysis)
    /// </summary>
    public class AC : FrequencySimulation
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the simulation</param>
        public AC(Identifier name) : base(name)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="frequencySweep">Frequency sweep</param>
        public AC(Identifier name, Sweep<double> frequencySweep) : base(name, frequencySweep)
        {
        }

        /// <summary>
        /// Execute
        /// </summary>
        protected override void Execute()
        {
            // Execute base behavior
            base.Execute();

            var state = RealState;
            var cstate = ComplexState;
            var baseconfig = BaseConfiguration;
            var freqconfig = FrequencyConfiguration;
            
            // Calculate the operating point
            cstate.Laplace = 0.0;
            state.UseIc = false;
            state.UseDc = true;
            state.Domain = RealState.DomainType.None;
            state.Gmin = baseconfig.Gmin;
            Op(baseconfig.DcMaxIterations);

            // Load all in order to calculate the AC info for all devices
            InitializeAcParameters();

            // Export operating point if requested
            var exportargs = new ExportDataEventArgs(this);
            if (freqconfig.KeepOpInfo)
                OnExport(exportargs);

            // Sweep the frequency
            foreach (var freq in FrequencySweep.Points)
            {
                // Calculate the current frequency
                cstate.Laplace = new Complex(0.0, 2.0 * Math.PI * freq);

                // Solve
                AcIterate();

                // Export the timepoint
                OnExport(exportargs);
            }
        }
    }
}
