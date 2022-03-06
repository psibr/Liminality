using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public interface IBeforeEnterHandler<TStateMachine, TSignal>
    where TStateMachine : StateMachine<TStateMachine>
    where TSignal : class
    {
        ValueTask BeforeEnterAsync(SignalContext<TStateMachine> context, TSignal signal, CancellationToken cancellationToken = default);
    }
}
