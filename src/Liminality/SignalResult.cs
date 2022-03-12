using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        object StartingState { get; }

        object Signal { get; }

        string GetNestedName(object value)
        {
            var type = value.GetType();
            Stack<string> names = new();

            do
            {
                names.Push(type.Name);

                if(type.DeclaringType is null) break;
                type = type.DeclaringType;

            } while (!IsTerminatingType(type));

            return string.Join(':', names);

            static bool IsTerminatingType(Type type)
            {
                return type.BaseType is not null
                    && type.BaseType.IsGenericType
                    && type.BaseType.GetGenericTypeDefinition() == typeof(StateMachine<>);
            }
        }
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

        public static AggregateSignalResult CreateResult(ISignalResult result, AggregateSignalResult? next = default)
        {
            if (result is AggregateSignalResult aggregate)
            {
                if (next is null) return aggregate;

                return new AggregateSignalResult(next.Concat(aggregate).ToList());
            }

            var currentResult = new List<ISignalResult> { result };

            if (next is null) return new AggregateSignalResult(currentResult);

            return new AggregateSignalResult(next.Concat(currentResult).ToList());
        }

        public static AggregateSignalResult Combine(AggregateSignalResult result!!, AggregateSignalResult next!!)
        {
            return new AggregateSignalResult(next.Concat(result).ToList());
        }
    }

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

        public override string ToString() => $"{((ISignalResult)this).GetNestedName(StartingState)} + {((ISignalResult)this).GetNestedName(Signal)} ---> {((ISignalResult)this).GetNestedName(NewState)}";
    }

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

        public override string ToString() => $"{((ISignalResult)this).GetNestedName(StartingState)} + {((ISignalResult)this).GetNestedName(Signal)} ---> {((ISignalResult)this).GetNestedName(NewState)}";
    }

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

        public override string ToString() => $"{((ISignalResult)this).GetNestedName(StartingState)} + {((ISignalResult)this).GetNestedName(Signal)} ---> !!";
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

        public override string ToString() => $"{((ISignalResult)this).GetNestedName(StartingState)} + {((ISignalResult)this).GetNestedName(Signal)} ---> ?";
    }
}
