using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PSIBR.Liminality
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class JsonInterfaceConverterAttribute : JsonConverterAttribute
    {
        public JsonInterfaceConverterAttribute(Type converterType)
            : base(converterType)
        {
        }
    }

    public class SignalResultConverter : JsonConverter<ISignalResult>
    {
        public override ISignalResult Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            ISignalResult value,
            JsonSerializerOptions options)
        {
            switch (value)
            {
                default:
                    {
                        JsonSerializer.Serialize(writer, value.ToString(), typeof(string), options);
                        break;
                    }
            }
        }
    }

    [JsonInterfaceConverter(typeof(SignalResultConverter))]
    public interface ISignalResult
    {

    }

    public class AggregateSignalResult
        : IReadOnlyList<ISignalResult>
    {
        public AggregateSignalResult(IReadOnlyList<ISignalResult> resultStack!!)
        {
            if (resultStack is null) throw new ArgumentNullException(nameof(resultStack));
            if (resultStack.Count == 0) throw new ArgumentException("Cannot be empty", nameof(resultStack));

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

    [DebuggerDisplay("{StartingState} + {Signal} ---> {NewState}")]
    public class TransitionedResult : ISignalResult
    {
        public TransitionedResult(object startingState, object signal, object newState)
        {
            StartingState = startingState;
            Signal = signal;
            NewState = newState;
        }

        public object StartingState { get; }

        public object Signal { get; }

        public object NewState { get; }

        protected string GetNestedName(object value)
        {
            var type = value.GetType();
            if (type.DeclaringType is null)
                return type.Name;
            else
                return type.DeclaringType.Name + "+" + type.Name;
        }

        public override string ToString() => $"{GetNestedName(StartingState)} + {GetNestedName(Signal)} ---> {GetNestedName(NewState)}";
    }

    [DebuggerDisplay("{StartingState} + {Signal} ---> {NewState} !!")]
    public class ExceptionThrownByAfterEntryHandlerResult : ISignalResult
    {
        public ExceptionThrownByAfterEntryHandlerResult(
            object startingState,
            object signal,
            object newState,
            Exception exception)
        {
            StartingState = startingState;
            Signal = signal;
            NewState = newState;

            if (exception is AggregateException aggregateException)
                HandlerExceptions = aggregateException;
            else
                HandlerExceptions = new AggregateException(exception);
        }

        public object StartingState { get; }

        public object Signal { get; }

        public object NewState { get; }

        public AggregateException HandlerExceptions { get; }

        public override string ToString() => $"{StartingState} + {Signal} ---> {NewState}";
    }

    [DebuggerDisplay("{StartingState} + {Signal} ---> !!")]
    public class ExceptionThrownByBeforeEnterHandlerResult : ISignalResult
    {
        public ExceptionThrownByBeforeEnterHandlerResult(
            object startingState,
            object signal,
            StateMachineStateMap.Transition transition,
            Exception preconditionException)
        {
            StartingState = startingState;
            Signal = signal;
            Transition = transition;

            if (preconditionException is AggregateException aggregateException)
                Exceptions = aggregateException;
            else
                Exceptions = new AggregateException(preconditionException);
        }

        public object StartingState { get; }

        public object Signal { get; }

        public StateMachineStateMap.Transition Transition { get; }

        public AggregateException Exceptions { get; }

        public override string ToString() => $"{StartingState} + {Signal} ---> !!";
    }

    [DebuggerDisplay("{StartingState} + {Signal} ---> ?")]
    public class TransitionNotFoundResult : ISignalResult
    {
        public TransitionNotFoundResult(object startingState, object signal)
        {
            StartingState = startingState;
            Signal = signal;
        }

        public object StartingState { get; }

        public object Signal { get; }

        public override string ToString() => $"{StartingState} + {Signal} ---> ?";
    }
}
