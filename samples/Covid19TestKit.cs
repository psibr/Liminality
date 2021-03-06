﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PSIBR.Liminality;

namespace Samples
{
    using static Samples.Covid19TestKit;

    public static class Covid19TestKitExtensions
    {
        public static void AddCovid19TestKitSample(this IServiceCollection services)
        {
            services.AddStateMachine<Covid19TestKit>(builder => builder
                .StartsIn<Ready>()
                .For<Ready>().On<BiologicalSequenceSample>().When<BiologicalSequenceIsntEmpty>().MoveTo<Analyzing>()
                .For<Analyzing>().On<Analysis>().MoveTo<Evaluating>()
                .For<Evaluating>().On<InconclusiveEvaluation>().MoveTo<Inconclusive>()
                .For<Evaluating>().On<NegativeEvaluation>().MoveTo<Negative>()
                .For<Evaluating>().On<PositiveEvaluation>().MoveTo<Positive>()
                .Build());
        }

        public static Covid19TestKit Create(this Engine<Covid19TestKit> engine)
        {
            return new Covid19TestKit(engine);
        }
    }

    public class Covid19TestKit : StateMachine<Covid19TestKit>
    {
        public Covid19TestKit(Engine<Covid19TestKit> engine)
            : base(engine)
        {
        }

        protected object State { get; private set; } = new Ready();

        public AggregateSignalResult Signal<TSignal>(TSignal signal)
        where TSignal : class, new()
        {
            var valueTask = base.SignalAsync<TSignal>(
                signal,
                cancellationToken => new ValueTask<object>(State),
                (state, cancellationToken) => { State = state; return new ValueTask(); },
                default);

            // handle synchronyously ONLY as an example of this capability
            if (valueTask.IsCompletedSuccessfully) return valueTask.Result;
            else if (valueTask.IsFaulted && valueTask.AsTask().Exception is Exception ex) throw ex;
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
            public string? Id { get; set; }

            public Sequence? Inst { get; set; }

            public class Sequence
            {
                public string? Data { get; set; }
            }

        }

        public class BiologicalSequenceIsntEmpty
            : IPrecondition<BiologicalSequenceSample>
        {
            public ValueTask<AggregateException?> CheckAsync(BiologicalSequenceSample signal, CancellationToken cancellationToken = default)
            {
                if (!string.IsNullOrWhiteSpace(signal.Inst?.Data))
                    return new ValueTask<AggregateException?>();

                return new ValueTask<AggregateException?>(new AggregateException(new EmptySequenceInstException()));
            }
        }

        public class Analyzing
            : ISignalHandler<Covid19TestKit, BiologicalSequenceSample>
        {
            /// <summary>
            /// Test for SARS-CoV-2
            /// </summary>
            /// <param name="sample"></param>
            /// <param name="cancellationToken"></param>
            public ValueTask<AggregateSignalResult?> InvokeAsync(SignalContext<Covid19TestKit> context, BiologicalSequenceSample sample, CancellationToken cancellationToken = default)
            {
                var result = new Analysis
                {
                    Orf1Gene = sample.Inst?.Data?.Contains("ORF1") ?? false,
                    NGene = sample.Inst?.Data?.Contains("N") ?? false,
                    EGene = sample.Inst?.Data?.Contains("E") ?? false
                };

                return new ValueTask<AggregateSignalResult?>(context.Self.Signal(result));
            }
        }

        public class Analysis
        {
            public bool Orf1Gene { get; set; }

            public bool NGene { get; set; }

            public bool EGene { get; set; }
        }

        public class Evaluating
            : ISignalHandler<Covid19TestKit, Analysis>
        {
            public ValueTask<AggregateSignalResult?> InvokeAsync(SignalContext<Covid19TestKit> context, Analysis analysis, CancellationToken cancellationToken = default)
            {
                if (analysis.Orf1Gene && analysis.NGene && analysis.EGene)
                {
                    return new ValueTask<AggregateSignalResult?>(context.Self.Signal(new PositiveEvaluation()));
                }
                else if (!analysis.Orf1Gene && !analysis.NGene && !analysis.EGene)
                {
                    return new ValueTask<AggregateSignalResult?>(context.Self.Signal(new NegativeEvaluation()));
                }
                else
                {
                    return new ValueTask<AggregateSignalResult?>(context.Self.Signal(new InconclusiveEvaluation()));
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
