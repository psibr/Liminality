using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public interface IOnEnterHandler<TStateMachine, TSignal>
    where TStateMachine : StateMachine<TStateMachine>
    where TSignal : class
    {
        ValueTask<AggregateSignalResult?> OnEnterAsync(SignalContext<TStateMachine> context, TSignal signal, CancellationToken cancellationToken = default);
    }
}
