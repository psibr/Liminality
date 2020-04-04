using System;

namespace PSIBR.Liminality
{
    public class Resolver : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;

        public Resolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TransitionResolution<TSignal>? ResolveTransition<TSignal>(StateMachineDefinition definition, Type stateType)
        where TSignal : class, new()
        {
            if(!definition.TryGetValue(new StateMachineDefinition.Input(stateType: stateType, signalType: typeof(TSignal)), out var transition))
                return default;

            IPrecondition<TSignal>? precondition = !(transition.PreconditionType is null)
                ? _serviceProvider.GetService(transition.PreconditionType) as IPrecondition<TSignal> 
                : null;
                
            var state = _serviceProvider.GetService(transition.NewStateType);

            if(state is null) throw new Exception($"Failed to resolve state: {transition.NewStateType.AssemblyQualifiedName}.");

            return new TransitionResolution<TSignal>(transition, precondition, state);
        }

        void IDisposable.Dispose()
        {
        }
    }

    public class TransitionResolution<TSignal>
        where TSignal : class, new()
    {
        public StateMachineDefinition.Transition Transition { get; }

        public IPrecondition<TSignal>? Precondition;

        public object State;

        public ISignalHandler<TSignal>? Handler;

        public TransitionResolution(StateMachineDefinition.Transition transition, IPrecondition<TSignal>? precondition, object state)
        {
            Transition = transition;
            Precondition = precondition;
            State = state;
            Handler = state as ISignalHandler<TSignal>;
        }
    }
}
