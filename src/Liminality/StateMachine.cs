using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public abstract class StateMachine<TStateMachine> where TStateMachine : StateMachine<TStateMachine>
    {
        private readonly Resolver _resolver;
        protected readonly StateMachineDefinition Definition;

        protected StateMachine(Resolver resolver, StateMachineDefinition<TStateMachine> definition)
        {
            _resolver = resolver;
            Definition = definition;
        }

        public async ValueTask<(bool DidTransition, object State)> SignalAsync<TSignal>(
            TSignal signal,
            CancellationToken cancellationToken = default)
            where TSignal : class, new()
        {
            var currentState = await LoadStateAsync(cancellationToken).ConfigureAwait(false);

            var resolution = _resolver.Resolve<TSignal>(Definition, currentState.GetType());

            if (resolution is null) return (false, currentState);

            var preconditionValueTask = resolution.Value.Precondition?.CheckAsync(signal, cancellationToken);

            if (preconditionValueTask != null && !await preconditionValueTask.Value.ConfigureAwait(false)) return (false, currentState);

            if (!await PersistStateAsync(resolution.Value.State, cancellationToken).ConfigureAwait(false))
                return (false, currentState);

            var handlerValueTask = resolution.Value.State.InvokeAsync(signal, cancellationToken);

            await handlerValueTask.ConfigureAwait(false);

            return (true, resolution.Value.State);
        }

        protected abstract ValueTask<object> LoadStateAsync(CancellationToken cancellationToken = default);

        protected abstract ValueTask<bool> PersistStateAsync(object state, CancellationToken cancellationToken = default);
    }
}
