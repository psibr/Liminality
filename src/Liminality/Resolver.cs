using System;

namespace PSIBR.Liminality
{

    public class Resolver<TStateMachine>
        : IDisposable
    where TStateMachine : StateMachine<TStateMachine>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly StateMachineDefinition _stateMachineDefinition;

        public Resolver(IServiceProvider serviceProvider, StateMachineDefinition<TStateMachine> stateMachineDefinition)
        {
            _serviceProvider = serviceProvider;
            _stateMachineDefinition = stateMachineDefinition;
        }

        public TransitionResolution<TStateMachine, TSignal>? ResolveTransition<TSignal>(Type stateType)
        where TSignal : class, new()
        {
            if (!_stateMachineDefinition.TryGetValue(new StateMachineDefinition.Input(stateType: stateType, signalType: typeof(TSignal)), out var transition))
                return default;

            IPrecondition<TSignal>? precondition = !(transition.PreconditionType is null)
                ? _serviceProvider.GetService(transition.PreconditionType) as IPrecondition<TSignal>
                : null;

            var state = _serviceProvider.GetService(transition.NewStateType);

            if (state is null) throw new Exception($"Failed to resolve state: {transition.NewStateType.AssemblyQualifiedName}.");

            return new TransitionResolution<TStateMachine, TSignal>(transition, precondition, state);
        }

        void IDisposable.Dispose()
        {
        }
    }

    public class TransitionResolution<TStateMachine, TSignal>
    where TStateMachine : StateMachine<TStateMachine>
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
