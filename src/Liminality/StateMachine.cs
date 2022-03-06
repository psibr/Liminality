using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public abstract class StateMachine<TStateMachine>
    where TStateMachine : StateMachine<TStateMachine>
    {
        private readonly LiminalEngine _engine;
        protected StateMachine(LiminalEngine engine, StateMachineDefinition<TStateMachine> definition)
        {
            _engine = engine;
            Definition = definition;
        }

        public StateMachineDefinition<TStateMachine> Definition { get; }

        protected ValueTask<AggregateSignalResult> SignalAsync<TSignal>(
            TSignal signal,
            Func<CancellationToken, ValueTask<object>> loadStateFunc,
            Func<object, CancellationToken, ValueTask> persistStateFunc,
            CancellationToken cancellationToken = default)
        where TSignal : class, new()
        {
            return _engine.SignalAsync((TStateMachine)this, signal, loadStateFunc, persistStateFunc, cancellationToken);
        }
    }
}
