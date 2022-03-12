using PSIBR.Liminality;
using System.Threading;
using System.Threading.Tasks;

namespace Samples
{
    public partial class OperationOrchestrator
    {

        [Transition<Request.Acknowledgement, Requested>]
        public class Requesting
            : IAfterEnterHandler<OperationOrchestrator, Request>
        {
            public async ValueTask<AggregateSignalResult?> AfterEnterAsync(SignalContext<OperationOrchestrator> context, Request signal, CancellationToken cancellationToken = default)
            {
                //do work maybe publish to a queue and get a confirmation...

                return await context.Self.SignalAsync(new Request.Acknowledgement()).ConfigureAwait(false);
            }

            [Transition<Start, InProgress>]
            [Transition<Cancel, Cancelled>]
            public class Requested
            {
            }
        }

    }
}
