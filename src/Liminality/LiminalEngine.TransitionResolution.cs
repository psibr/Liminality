namespace PSIBR.Liminality
{
    public sealed partial class LiminalEngine
    {
        private sealed class TransitionResolution<TStateMachine, TSignal>
        where TStateMachine : StateMachine<TStateMachine>
        where TSignal : class, new()
        {
            public StateMachineStateMap.Transition Transition { get; }

            public IBeforeEnterHandler<TStateMachine, TSignal>? BeforeEnterHandler;

            public object State;

            public IOnEnterHandler<TStateMachine, TSignal>? Handler;

            public TransitionResolution(StateMachineStateMap.Transition transition, IBeforeEnterHandler<TStateMachine, TSignal>? precondition, object state)
            {
                Transition = transition;
                BeforeEnterHandler = precondition;
                State = state;
                Handler = state as IOnEnterHandler<TStateMachine, TSignal>;
            }
        }
    }
}
