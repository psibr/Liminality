using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using PSIBR.Liminality;

namespace AspNetCoreExample
{    public class SARSCoV2Assay : StateMachine<SARSCoV2Assay>
    {
        public SARSCoV2Assay(
            string id,

            /* Here you could add a repository or eventstream as a dependency */
            Engine<SARSCoV2Assay> engine)
            : base(engine)
        {
        }

        protected object State { get; private set; } = new Ready();

        public ISignalResult Signal<TSignal>(TSignal signal)
        where TSignal : class, new()
        {
            var valueTask = base.SignalAsync<TSignal>(
                signal,
                cancellationToken => new ValueTask<object>(State),
                (state, cancellationToken) => { State = state; return new ValueTask(); },
                default);

            // handle synchronyously ONLY as an example of this capability
            if (valueTask.IsCompletedSuccessfully) return valueTask.Result;
            else if (valueTask.IsFaulted) throw valueTask.AsTask().Exception;
            else throw new NotSupportedException("This state machine cannot execute async");
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

            public Sequence Inst { get; set; }

            public class Sequence
            {
                public string Data { get; set; }
            }

        }

        public class Analyzing
            : ISignalHandler<SARSCoV2Assay, BiologicalSequenceSample>
        {
            /// <summary>
            /// Test for SARS-CoV-2
            /// </summary>
            /// <param name="sample"></param>
            /// <param name="cancellationToken"></param>
            public ValueTask<ISignalResult> InvokeAsync(SignalContext<SARSCoV2Assay> context, BiologicalSequenceSample sample, CancellationToken cancellationToken = default)
            {
                var result = new Analysis
                {
                    Orf1Gene = sample.Inst.Data.Contains("ORF1"),
                    NGene = sample.Inst.Data.Contains("N"),
                    EGene = sample.Inst.Data.Contains("E")
                };

                return new ValueTask<ISignalResult>(context.Self.Signal(result));
            }
        }

        public class Analysis
        {
            public bool Orf1Gene { get; set; }

            public bool NGene { get; set; }

            public bool EGene { get; set; }
        }

        public class Evaluating
            : ISignalHandler<SARSCoV2Assay, Analysis>
        {
            public ValueTask<ISignalResult> InvokeAsync(SignalContext<SARSCoV2Assay> context, Analysis analysis, CancellationToken cancellationToken = default)
            {
                if (analysis.Orf1Gene && analysis.NGene && analysis.EGene)
                {
                    return new ValueTask<ISignalResult>(context.Self.Signal(new PositiveEvaluation()));
                }
                else if (!analysis.Orf1Gene && !analysis.NGene && !analysis.EGene)
                {
                    return new ValueTask<ISignalResult>(context.Self.Signal(new NegativeEvaluation()));
                }
                else
                {
                    return new ValueTask<ISignalResult>(context.Self.Signal(new InconclusiveEvaluation()));
                }

            }
        }

        public class PositiveEvaluation { }

        public class NegativeEvaluation { }

        public class InconclusiveEvaluation { }

        public class Positive { }

        public class Negative { }

        public class Inconclusive { }

        public class EmptySequenceInstException : Exception
        {

        }
    }
}
