using System;
using System.Collections;
using System.Collections.Generic;

namespace PSIBR.Liminality
{
    public interface ISignalResult { }

    public class AggregateSignalResult 
        : IReadOnlyList<ISignalResult>
    {
        public AggregateSignalResult(IReadOnlyList<ISignalResult> resultStack)
        {
            if(resultStack is null) throw new ArgumentNullException(nameof(resultStack));
            if(resultStack.Count == 0) throw new ArgumentException("Cannot be empty", nameof(resultStack));

            InnerResults = resultStack;

            InnerResult = InnerResults[0];
        }

        public ISignalResult InnerResult { get; }

        public IReadOnlyList<ISignalResult> InnerResults { get; }

        public int Count => InnerResults.Count;

        public ISignalResult this[int index] => InnerResults[index];

        public IEnumerator<ISignalResult> GetEnumerator()
        {
            return InnerResults.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)InnerResults).GetEnumerator();
        }
    }

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

    public class ExceptionThrownByHandlerResult : ISignalResult
    {
        public ExceptionThrownByHandlerResult(
            object startingState,
            object signal,
            object newState,
            Exception exception)
        {
            StartingState = startingState;
            Signal = signal;
            NewState = newState;
            
            if(exception is AggregateException aggregateException)
                HandlerExceptions = aggregateException;
            else
                HandlerExceptions = new AggregateException(exception);
        }

        public object StartingState { get; }

        public object Signal { get; }

        public object NewState { get; }

        public AggregateException HandlerExceptions { get; }
    }

    public class ExceptionThrownByPreconditionResult : ISignalResult
    {
        public ExceptionThrownByPreconditionResult(
            object startingState,
            object signal,
            StateMachineDefinition.Transition transition,
            Exception preconditionException)
        {
            StartingState = startingState;
            Signal = signal;
            Transition = transition;
            
            if(preconditionException is AggregateException aggregateException)
                PreconditionExceptions = aggregateException;
            else
                PreconditionExceptions = new AggregateException(preconditionException);
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
