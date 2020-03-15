using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    using TransitionMap = Dictionary<(Type CurrentState, Type Signal), (Type? Precondition, Type NewState)>;

    public abstract class StateMachine
    {
        private readonly Resolver _resolver;
        private readonly Lazy<TransitionMap> _transitionMap;

        protected StateMachine(Resolver resolver)
        {
            _resolver = resolver;
            _transitionMap = new Lazy<TransitionMap>(valueFactory: DefineApplied);

            // Use partial application to provide StateMachineBuilder to the Define method lazily
            // https://en.wikipedia.org/wiki/Partial_application
            TransitionMap DefineApplied()
            {
                return Define(new StateMachineBuilder());
            }
        }

        public async ValueTask<(bool DidTransition, object State)> SignalAsync<TSignal>(
            TSignal signal,
            CancellationToken cancellationToken = default)
            where TSignal : class, new()
        {
            var currentState = await LoadStateAsync(cancellationToken).ConfigureAwait(false);

            var (foundMatch, resolution) = _resolver.Resolve<TSignal>(_transitionMap.Value, currentState.GetType());

            if (!foundMatch) return (false, currentState);

            var preconditionValueTask = resolution.Precondition.CheckAsync(signal, cancellationToken);

            if (!await preconditionValueTask.ConfigureAwait(false)) return (false, currentState);

            if (!await PersistStateAsync(resolution.State, cancellationToken).ConfigureAwait(false))
                return (false, currentState);

            var handlerValueTask = resolution.State.InvokeAsync(signal, cancellationToken);

            await handlerValueTask.ConfigureAwait(false);

            return (true, resolution.State);
        }

        protected abstract TransitionMap Define(StateMachineBuilder stateMachineBuilder);

        protected abstract ValueTask<object> LoadStateAsync(CancellationToken cancellationToken = default);

        protected abstract ValueTask<bool> PersistStateAsync(object state, CancellationToken cancellationToken = default);
    }
}
