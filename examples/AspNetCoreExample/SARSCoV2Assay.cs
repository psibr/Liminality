﻿using PSIBR.Liminality;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreExample
{
    public class SARSCoV2Assay : StateMachine<SARSCoV2Assay>
    {
        public SARSCoV2Assay(
            /* Here you could add a repository or eventstream as a dependency */
            Resolver resolver, StateMachineDefinition<SARSCoV2Assay> stateMachineDefinition)
            : base(resolver, stateMachineDefinition)
        {
        }

        protected object State { get; private set; } = new Ready();

        protected override ValueTask<object> LoadStateAsync(CancellationToken cancellationToken = default)
        {
            return new ValueTask<object>(State);
        }

        protected override ValueTask<bool> PersistStateAsync(object state, CancellationToken cancellationToken = default)
        {
            State = state;

            return new ValueTask<bool>(true);
        }

        /// <summary>
        /// Empty state
        /// </summary>
        public class Ready { }

        /// <summary>
        /// A theoretical biological sequence sample.
        /// https://www.ncbi.nlm.nih.gov/IEB/ToolBox/SDKDOCS/DATAMODL.HTML
        /// </summary>
        /// <remarks>This isn't fully implemented, just enough for an example</remarks>
        public class BiologicalSequenceSample
        {
            [Required]
            public string Id { get; set; }

            public string Descr { get; set; }

            public Sequence Inst { get; set; }

            public class Sequence
            {
                public string Data { get; set; }
            }

        }

        public class Analyzing
            : ISignalHandler<BiologicalSequenceSample>
        {
            /// <summary>
            /// Test for SARS-CoV-2
            /// </summary>
            /// <param name="sample"></param>
            /// <param name="cancellationToken"></param>
            public ValueTask InvokeAsync(BiologicalSequenceSample sample, CancellationToken cancellationToken = default)
            {
                var result = new Analysis
                {
                    Orf1Gene = sample.Inst.Data.Contains("ORF1"),
                    NGene = sample.Inst.Data.Contains("N"),
                    EGene = sample.Inst.Data.Contains("E")
                };

                // Signal with analysis

                return new ValueTask();
            }
        }

        public class Analysis
        {
            public bool Orf1Gene { get; set; }

            public bool NGene { get; set; }

            public bool EGene { get; set; }
        }

        public class Evaluating
            : ISignalHandler<Analysis>
        {
            public ValueTask InvokeAsync(Analysis analysis, CancellationToken cancellationToken = default)
            {
                if (analysis.Orf1Gene && analysis.NGene && analysis.EGene)
                {
                    // signal PositiveEvaluation
                    return new ValueTask();
                }
                else if (!analysis.Orf1Gene && !analysis.NGene && !analysis.EGene)
                {
                    // signal NegativeEvaluation
                    return new ValueTask();
                }
                else
                {
                    // signal InconclusiveEvaluation
                    return new ValueTask();
                }

            }
        }

        public class PositiveEvaluation { }

        public class NegativeEvaluation { }

        public class InconclusiveEvaluation { }

        public class Positive { }

        public class Negative { }

        public class Inconclusive { }
    }
}