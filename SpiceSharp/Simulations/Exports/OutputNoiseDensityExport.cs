﻿using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export the output noise density.
    /// </summary>
    /// <seealso cref="Export{S, T}" />
    public class OutputNoiseDensityExport : Export<INoiseSimulation, double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutputNoiseDensityExport"/> class.
        /// </summary>
        /// <param name="noise">The noise analysis.</param>
        public OutputNoiseDensityExport(INoiseSimulation noise)
            : base(noise)
        {
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            INoiseSimulationState state = Simulation.GetState<INoiseSimulationState>();
            Extractor = () => state.OutputNoiseDensity;
        }
    }
}
