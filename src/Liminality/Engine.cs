using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{

    public class Engine<TStateMachine>
    where TStateMachine : StateMachine<TStateMachine>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly StateMachineDefinition _stateMachineDefinition;

        public Engine(IServiceProvider serviceProvider, StateMachineDefinition<TStateMachine> stateMachineDefinition)
        {
            _serviceProvider = serviceProvider;
            _stateMachineDefinition = stateMachineDefinition;
        }

        public TransitionResolution<TSignal>? ResolveTransition<TSignal>(Type stateType)
        where TSignal : class, new()
        {
            if (!_stateMachineDefinition.TryGetValue(new StateMachineDefinition.Input(stateType: stateType, signalType: typeof(TSignal)), out var transition))
                return default;

            IPrecondition<TSignal>? precondition = !(transition.PreconditionType is null)
                ? _serviceProvider.GetService(transition.PreconditionType) as IPrecondition<TSignal>
                : null;

            var state = _serviceProvider.GetService(transition.NewStateType);

            if (state is null) throw new Exception($"Failed to resolve state: {transition.NewStateType.AssemblyQualifiedName}.");

            return new TransitionResolution<TSignal>(transition, precondition, state);
        }

        public TStateMachine CreateStateMachine(Func<Engine<TStateMachine>, TStateMachine> factoryFunc)
        {
            var stateMachine = factoryFunc(this);

            return stateMachine;
        }

        public async ValueTask<ISignalResult> SignalAsync<TSignal>(
            TStateMachine self,
            TSignal signal,
            Func<CancellationToken, ValueTask<object>> loadStateFunc,
            Func<object, CancellationToken, ValueTask> persistStateFunc,
            CancellationToken cancellationToken = default)
        where TSignal : class, new()
        {
            var loadStateValueTask = loadStateFunc(cancellationToken);
            if(!loadStateValueTask.IsCompletedSuccessfully) await loadStateValueTask.ConfigureAwait(false);

            var startingState = loadStateValueTask.Result;

            var resolution = ResolveTransition<TSignal>(startingState.GetType());

            if (resolution is null) return new TransitionNotFoundResult(startingState, signal);

            if (!(resolution.Precondition is null))
            {
                var preconditionValueTask = resolution.Precondition.CheckAsync(signal, cancellationToken);
                // TODO: should catch any exceptions here
                if (!preconditionValueTask.IsCompletedSuccessfully) await preconditionValueTask.ConfigureAwait(false);

                if (!(preconditionValueTask.Result is null)) return new RejectedByPreconditionResult(startingState, signal, resolution.Transition, preconditionValueTask.Result);
            }

            var persistStateValueTask = persistStateFunc(resolution.State, cancellationToken);
            if(!persistStateValueTask.IsCompletedSuccessfully) await persistStateValueTask.ConfigureAwait(false);

            if (resolution.Handler is null) return new TransitionedResult(startingState, resolution.State);
            
            var handlerValueTask = resolution.Handler.InvokeAsync(new SignalContext<TStateMachine>(self, startingState, resolution.State), signal, cancellationToken);

            // TODO: should catch any exceptions here
            if (!handlerValueTask.IsCompletedSuccessfully) await handlerValueTask.ConfigureAwait(false);

            // replace with a multiple transition result?
            return new TransitionedResult(startingState, resolution.State);
        }

            public class TransitionResolution<TSignal>
            where TSignal : class, new()
            {
                public StateMachineDefinition.Transition Transition { get; }

                public IPrecondition<TSignal>? Precondition;

                public object State;

                public ISignalHandler<TStateMachine, TSignal>? Handler;

                public TransitionResolution(StateMachineDefinition.Transition transition, IPrecondition<TSignal>? precondition, object state)
                {
                    Transition = transition;
                    Precondition = precondition;
                    State = state;
                    Handler = state as ISignalHandler<TStateMachine, TSignal>;
                }
            }
    }
}
