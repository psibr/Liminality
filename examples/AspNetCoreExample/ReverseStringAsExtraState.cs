using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PSIBR.Liminality;


namespace AspNetCoreExample
{
    using static AspNetCoreExample.ReverseStringAsExtraState;

    public static class ReverseStringAsExtraStateExtensions
    {
        public static void AddReverseStringAsExtraStateExample(this IServiceCollection services)
        {
            services.AddStateMachine<ReverseStringAsExtraState>(builder => builder
                .StartsIn<Idle>()
                .For<Idle>().On<LoadValues>().MoveTo<Idle>()
                .For<Idle>().On<StartProcessing>().MoveTo<Processing>()
                .For<Processing>().On<ReportResult>().MoveTo<Finished>()
                .Build());
        }

        public static ReverseStringAsExtraState Create(this Engine<ReverseStringAsExtraState> engine)
        {
            return new ReverseStringAsExtraState(engine);
        }
    }

    public class ReverseStringAsExtraState : StateMachine<ReverseStringAsExtraState>
    {
        public ReverseStringAsExtraState(Engine<ReverseStringAsExtraState> engine) : base(engine) { }

        public string Input { get; private set;}

        public string Output { get; private set;}

        public object State { get; private set; } = new Idle();

        public async ValueTask<AggregateSignalResult> SignalAsync<TSignal>(TSignal signal, CancellationToken cancellationToken = default)
        where TSignal : class, new()
        {
            var valueTask = base.SignalAsync<TSignal>(
                signal,
                cancellationToken => new ValueTask<object>(State),
                (state, cancellationToken) => 
                {
                    // In our persist function we can grab values from signals.
                    // We could also do this in any handler by assigning to the state machine
                    // and saving it here.
                    if(signal is LoadValues loadValuesSignal)
                        Input = loadValuesSignal.Input;
                    else if (signal is ReportResult resultReport)
                        Output = resultReport.Result;
                    
                    State = state;
                    
                    return new ValueTask();
                },
                default);

            // handle synchronyously ONLY as an example of this capability
            if (valueTask.IsCompletedSuccessfully) return valueTask.Result;

            return await valueTask.ConfigureAwait(false);
        }

        public class Idle { }

        public class Processing
            : ISignalHandler<ReverseStringAsExtraState, StartProcessing>
        {
            private static IEnumerable<string> GraphemeClusters(string s) {
                var enumerator = System.Globalization.StringInfo.GetTextElementEnumerator(s);
                while(enumerator.MoveNext()) {
                    yield return (string)enumerator.Current;
                }
            }
            private static string ReverseGraphemeClusters(string s) {
                return string.Join("", GraphemeClusters(s).Reverse().ToArray());
            }

            public ValueTask<AggregateSignalResult> InvokeAsync(
                SignalContext<ReverseStringAsExtraState> context,
                StartProcessing signal,
                CancellationToken cancellationToken = default)
            {
                return context.Self.SignalAsync(new ReportResult { Result =  ReverseGraphemeClusters(context.Self.Input)} , cancellationToken);
            }
        }

        public class Finished { }

        public class LoadValues
        {
            public string Input { get; set; }
        }

        public class StartProcessing { }

        public class ReportResult 
        {
            public string Result { get; set; }
        }

    }
}