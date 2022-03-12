using PSIBR.Liminality;
using System.Threading;
using System.Threading.Tasks;

namespace Samples
{
    public partial class OperationOrchestrator
    {
        [Transition<Ping, InProgress>]
        [Transition<Pause, Pausing>]
        [Transition<Complete, Completed>]
        [Transition<Cancel, Cancelling>]
        [Transition<Throw, Failed>]
        public class InProgress
            : IAfterEnterHandler<OperationOrchestrator, Start>
        {
            public ValueTask<AggregateSignalResult?> AfterEnterAsync(SignalContext<OperationOrchestrator> context, Start signal, CancellationToken cancellationToken = default)
            {
                //do work

                // if the work is in the state machine,
                // you probably want some extra form of checkpointing and break up long processes.
                // Additionally you could add cancellation via a subscription your state store on the Cancelling state's after entry action
                return default;
            }
        }

    }
}
