using System;

namespace PSIBR.Liminality
{
    public interface ISignalResult { }

    public class TransitionedResult : ISignalResult
    {
        public TransitionedResult(object startingState, object newState)
        {
            StartingState = startingState;
            NewState = newState;
        }

        public object StartingState { get; }

        public object NewState { get; }
    }

    public class RejectedByPreconditionResult : ISignalResult
    {
        public RejectedByPreconditionResult(
            object startingState,
            object signal,
            StateMachineDefinition.Transition transition,
            AggregateException preconditionExceptions)
        {
            StartingState = startingState;
            Signal = signal;
            Transition = transition;
            PreconditionExceptions = preconditionExceptions;
        }

        public object StartingState { get; }

        public object Signal { get; }

        public StateMachineDefinition.Transition Transition { get; }

        public AggregateException PreconditionExceptions { get; }
    }

    public class TransitionNotFoundResult : ISignalResult
    {
        public TransitionNotFoundResult(object startingState, object signal)
        {
            StartingState = startingState;
            Signal = signal;
        }

        public object StartingState { get; }

        public object Signal { get; }
    }
}
