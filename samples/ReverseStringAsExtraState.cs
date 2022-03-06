using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PSIBR.Liminality;


namespace Samples
{
    using static Samples.ReverseStringAsExtraState;

    public static class ReverseStringAsExtraStateExtensions
    {
        public static void AddReverseStringAsExtraStateSample(this IServiceCollection services)
        {
            services.AddStateMachineDependencies<ReverseStringAsExtraState>(builder => builder
                .StartsIn<Idle>()
                .For<Idle>().On<LoadValues>().MoveTo<Idle>()
                .For<Idle>().On<StartProcessing>().MoveTo<Processing>()
                .For<Processing>().On<ReportResult>().MoveTo<Finished>()
                .Build());

            services.AddTransient<ReverseStringAsExtraState>();
        }
    }

    public class ReverseStringAsExtraState : StateMachine<ReverseStringAsExtraState>
    {
        public ReverseStringAsExtraState(LiminalEngine engine, StateMachineDefinition<ReverseStringAsExtraState> definition) : base(engine, definition) { }

        public string Input { get; private set; } = string.Empty;

        public string? Output { get; private set; }

        public object State { get; private set; } = new Idle();

        public async ValueTask<AggregateSignalResult?> SignalAsync<TSignal>(TSignal signal, CancellationToken cancellationToken = default)
        where TSignal : class, new()
        {
            var valueTask = SignalAsync(
                signal,
                cancellationToken => new ValueTask<object>(State),
                (state, cancellationToken) =>
                {
                    // In our persist function we can grab values from signals.
                    // We could also do this in any handler by assigning to the state machine
                    // and saving it here.
                    if (signal is LoadValues loadValuesSignal)
                        Input = loadValuesSignal.Input ?? string.Empty;
                    else if (signal is ReportResult resultReport)
                        Output = resultReport.Result;

                    State = state;

                    return new ValueTask();
                },
                cancellationToken);

            return await valueTask.ConfigureAwait(false);
        }

        public class Idle { }

        public class Processing
            : IAfterEnterHandler<ReverseStringAsExtraState, StartProcessing>
        {
            private static IEnumerable<string> GraphemeClusters(string s)
            {
                var enumerator = System.Globalization.StringInfo.GetTextElementEnumerator(s);
                while (enumerator.MoveNext())
                {
                    yield return (string)enumerator.Current;
                }
            }
            private static string ReverseGraphemeClusters(string s)
            {
                return string.Join(string.Empty, GraphemeClusters(s).Reverse().ToArray());
            }

            public ValueTask<AggregateSignalResult?> AfterEnterAsync(
                SignalContext<ReverseStringAsExtraState> context,
                StartProcessing signal,
                CancellationToken cancellationToken = default)
            {
                return context.Self.SignalAsync(new ReportResult { Result = ReverseGraphemeClusters(context.Self.Input) }, cancellationToken);
            }
        }

        public class Finished { }

        public class LoadValues
        {
            public string? Input { get; set; }
        }

        public class StartProcessing { }

        public class ReportResult
        {
            public string? Result { get; set; }
        }

    }
}