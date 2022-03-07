using System;
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
            services.AddStateMachineDependenciesFromAttributes<Covid19TestKit>();

            services.AddScoped<Factory>();
        }
    }

    [InitialState<Ready>]
    public class Covid19TestKit : StateMachine<Covid19TestKit>
    {
        public class Factory
        {
            private readonly LiminalEngine _engine;
            private readonly StateMachineDefinition<Covid19TestKit> _definition;

            public Factory(LiminalEngine engine, StateMachineDefinition<Covid19TestKit> definition)
            {
                _engine = engine;
                _definition = definition;
            }

            public Covid19TestKit CreateKit() => new(_engine, _definition);
        }

        public Covid19TestKit(LiminalEngine engine, StateMachineDefinition<Covid19TestKit> definition)
            : base(engine, definition)
        {
        }

        protected object State { get; private set; } = new Ready();

        public AggregateSignalResult Signal<TSignal>(TSignal signal)
        where TSignal : class, new()
        {
            var valueTask = SignalAsync(
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
        [Transition<BiologicalSequenceSample, Analyzing>]
        public class Ready { }

        /// <summary>
        /// A theoretical biological sequence sample.
        /// https://www.ncbi.nlm.nih.gov/IEB/ToolBox/SDKDOCS/DATAMODL.HTML
        /// </summary>
        /// <remarks>This isn't fully implemented, just enough for an example</remarks>
        public class BiologicalSequenceSample : IFormattable
        {
            [Required]
            public string? Id { get; set; }

            public Sequence? Inst { get; set; }

            public string ToString(string? format, IFormatProvider? formatProvider)
            {
                throw new NotImplementedException();
            }

            public class Sequence
            {
                public string? Data { get; set; }
            }

        }

        [Transition<Analysis, Evaluating>]
        public class Analyzing
            : IBeforeEnterHandler<Covid19TestKit, BiologicalSequenceSample>
            , IAfterEnterHandler<Covid19TestKit, BiologicalSequenceSample>
        {
            public ValueTask BeforeEnterAsync(SignalContext<Covid19TestKit> context, BiologicalSequenceSample signal, CancellationToken cancellationToken = default)
            {
                if (!string.IsNullOrWhiteSpace(signal.Inst?.Data))
                    return ValueTask.CompletedTask; // return empty task for no error

                throw new EmptySequenceInstException();
            }

            /// <summary>
            /// Test for SARS-CoV-2
            /// </summary>
            /// <param name="sample"></param>
            /// <param name="cancellationToken"></param>
            [PossibleSignal<Analysis>]
            public ValueTask<AggregateSignalResult?> AfterEnterAsync(SignalContext<Covid19TestKit> context, BiologicalSequenceSample sample, CancellationToken cancellationToken = default)
            {
                var result = new Analysis
                {
                    Orf1Gene = sample.Inst?.Data?.Contains("ORF1") ?? false,
                    NGene = sample.Inst?.Data?.Contains('N') ?? false,
                    EGene = sample.Inst?.Data?.Contains('E') ?? false
                };

                return new(context.Self.Signal(result));
            }
        }

        public class Analysis
        {
            public bool Orf1Gene { get; set; }

            public bool NGene { get; set; }

            public bool EGene { get; set; }
        }

        [Transition<InconclusiveEvaluation, Inconclusive>]
        [Transition<NegativeEvaluation, Negative>]
        [Transition<Positive, Positive>]
        public class Evaluating
            : IAfterEnterHandler<Covid19TestKit, Analysis>
        {
            public ValueTask<AggregateSignalResult?> AfterEnterAsync(SignalContext<Covid19TestKit> context, Analysis analysis, CancellationToken cancellationToken = default)
            {
                if (analysis.Orf1Gene && analysis.NGene && analysis.EGene)
                {
                    return new(context.Self.Signal(new PositiveEvaluation()));
                }
                else if (!analysis.Orf1Gene && !analysis.NGene && !analysis.EGene)
                {
                    return new(context.Self.Signal(new NegativeEvaluation()));
                }
                else
                {
                    return new(context.Self.Signal(new InconclusiveEvaluation()));
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
