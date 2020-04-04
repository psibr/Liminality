using Microsoft.Extensions.DependencyInjection;
using PSIBR.Liminality;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreExample
{
    public class SARSCoV2AssayFactory
    {
        private readonly StateMachineFactory _stateMachineFactory;

        public SARSCoV2AssayFactory(StateMachineFactory stateMachineFactory) 
        {
            _stateMachineFactory = stateMachineFactory;
        }

        public StateMachineScope<SARSCoV2Assay> CreateScoped(string id)
        {
            return _stateMachineFactory.CreateScopedStateMachine<SARSCoV2Assay>((resolver, definition) => new SARSCoV2Assay(id, resolver, definition));
        }

        public SARSCoV2Assay Create(string id)
        {
            return _stateMachineFactory.CreateStateMachine<SARSCoV2Assay>((resolver, definition) => new SARSCoV2Assay(id, resolver, definition));
        }
    }

    public class SARSCoV2Assay : StateMachine<SARSCoV2Assay>
    {
        public SARSCoV2Assay(
            string id,

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

        protected override ValueTask PersistStateAsync(object state, CancellationToken cancellationToken = default)
        {
            State = state;

            return new ValueTask();
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
            : ISignalHandler<BiologicalSequenceSample>
        {
            /// <summary>
            /// Test for SARS-CoV-2
            /// </summary>
            /// <param name="sample"></param>
            /// <param name="cancellationToken"></param>
            public ValueTask<ISignalResult> InvokeAsync(SignalContext context, BiologicalSequenceSample sample, CancellationToken cancellationToken = default)
            {
                var result = new Analysis
                {
                    Orf1Gene = sample.Inst.Data.Contains("ORF1"),
                    NGene = sample.Inst.Data.Contains("N"),
                    EGene = sample.Inst.Data.Contains("E")
                };

                return context.Self.SignalAsync(result, cancellationToken);
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
            public ValueTask<ISignalResult> InvokeAsync(SignalContext context, Analysis analysis, CancellationToken cancellationToken = default)
            {
                if (analysis.Orf1Gene && analysis.NGene && analysis.EGene)
                {
                    return context.Self.SignalAsync(new PositiveEvaluation(), cancellationToken);
                }
                else if (!analysis.Orf1Gene && !analysis.NGene && !analysis.EGene)
                {
                    return context.Self.SignalAsync(new NegativeEvaluation(), cancellationToken);
                }
                else
                {
                    return context.Self.SignalAsync(new InconclusiveEvaluation(), cancellationToken);
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
