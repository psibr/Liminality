using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public interface IStateMachine
    {
        ValueTask<ISignalResult> SignalAsync<TSignal>(TSignal signal, CancellationToken cancellationToken = default) where TSignal : class, new();
        ValueTask<ISignalResult> SignalAsync<TSignal>(TSignal signal, SignalOptions signalOptions, CancellationToken cancellationToken = default) where TSignal : class, new();
    }

    public abstract class StateMachine<TStateMachine>
        : IStateMachine
    where TStateMachine : StateMachine<TStateMachine>
    {
        private readonly Resolver _resolver;
        protected readonly StateMachineDefinition<TStateMachine> Definition;

        protected StateMachine(Resolver resolver, StateMachineDefinition<TStateMachine> definition)
        {
            _resolver = resolver;
            Definition = definition;
        }

        public async ValueTask<ISignalResult> SignalAsync<TSignal>(
            TSignal signal,
            CancellationToken cancellationToken = default)
        where TSignal : class, new()
        {
            var signalValueTask = SignalAsync<TSignal>(signal, new SignalOptions(), cancellationToken: cancellationToken);

            if (!signalValueTask.IsCompletedSuccessfully) await signalValueTask.ConfigureAwait(false);

            return signalValueTask.Result;
        }

        public async ValueTask<ISignalResult> SignalAsync<TSignal>(
            TSignal signal,
            SignalOptions signalOptions,
            CancellationToken cancellationToken = default)
        where TSignal : class, new()
        {
            var startingState = await LoadStateAsync(cancellationToken).ConfigureAwait(false);

            TransitionResolution<TSignal>? resolution = _resolver.ResolveTransition<TSignal>(Definition, startingState.GetType());

            if (resolution is null) return new TransitionNotFoundResult(startingState, signal);

            if (!(resolution.Precondition is null))
            {
                var preconditionValueTask = resolution.Precondition.CheckAsync(signal, cancellationToken);

                // TODO: should catch any exceptions here
                if (!preconditionValueTask.IsCompletedSuccessfully) await preconditionValueTask.ConfigureAwait(false);

                if (!(preconditionValueTask.Result is null)) return new RejectedByPreconditionResult(startingState, signal, resolution.Transition, preconditionValueTask.Result);
            }

            await PersistStateAsync(resolution.State, cancellationToken).ConfigureAwait(false);

            if (resolution.Handler is null) return new TransitionedResult(startingState, resolution.State);
            
            var handlerValueTask = resolution.Handler.InvokeAsync(new SignalContext(this, startingState, resolution.State), signal, cancellationToken);

            // TODO: should catch any exceptions here
            if (!handlerValueTask.IsCompletedSuccessfully) await handlerValueTask.ConfigureAwait(false);


            // replace with a multiple transition result?
            return new TransitionedResult(startingState, resolution.State);
        }

        protected abstract ValueTask<object> LoadStateAsync(CancellationToken cancellationToken = default);

        protected abstract ValueTask PersistStateAsync(object state, CancellationToken cancellationToken = default);
    }
}
