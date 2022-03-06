namespace PSIBR.Liminality
{
    public class SignalContext<TStateMachine>
    where TStateMachine : StateMachine<TStateMachine>
    {
        public SignalContext(TStateMachine self, object startingState, object newState)
        {
            Self = self;
            StartingState = startingState;
            NewState = newState;
        }

        public TStateMachine Self { get; }

        public object StartingState { get; }

        public object NewState { get; }
    }
}
