using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public abstract class StateMachine<TStateMachine>
    where TStateMachine : StateMachine<TStateMachine>
    {
        private readonly Engine<TStateMachine> _engine;

        protected StateMachine(Engine<TStateMachine> engine)
        {
            _engine = engine;
        }

        protected ValueTask<ISignalResult> SignalAsync<TSignal>(
            TSignal signal,
            Func<CancellationToken, ValueTask<object>> loadStateFunc,
            Func<object, CancellationToken, ValueTask> persistStateFunc,
            CancellationToken cancellationToken = default)
        where TSignal : class, new()
        {
            return _engine.SignalAsync<TSignal>((TStateMachine)this, signal, loadStateFunc, persistStateFunc, cancellationToken);
        }
    }
}
