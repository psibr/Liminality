using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public interface IAfterEnterHandler<TStateMachine, TSignal>
    where TStateMachine : StateMachine<TStateMachine>
    where TSignal : class
    {
        ValueTask<AggregateSignalResult?> AfterEnterAsync(SignalContext<TStateMachine> context, TSignal signal, CancellationToken cancellationToken = default);
    }
}
