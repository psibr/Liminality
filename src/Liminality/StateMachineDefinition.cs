using System;
using System.Collections.Generic;

namespace PSIBR.Liminality
{
    public class StateMachineDefinition<TStateMachine> : StateMachineDefinition where TStateMachine : StateMachine<TStateMachine>
    {
        public StateMachineDefinition(Type initialState) : base(initialState)
        {
        }
    }

    public class StateMachineDefinition : Dictionary<StateMachineDefinition.Input, StateMachineDefinition.Transition>
    {
        public StateMachineDefinition(Type initialState)
        {
            InitialState = initialState;
        }

        public Type InitialState { get; }

        public struct Input
        {
            public Type CurrentStateType;
            public Type SignalType;

            public Input(Type stateType, Type signalType)
            {
                CurrentStateType = stateType;
                SignalType = signalType;
            }

            public override bool Equals(object? obj)
            {
                return obj is Input other &&
                        EqualityComparer<Type>.Default.Equals(CurrentStateType, other.CurrentStateType) &&
                        EqualityComparer<Type>.Default.Equals(SignalType, other.SignalType);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(CurrentStateType, SignalType);
            }

            public void Deconstruct(out Type currentState, out Type signal)
            {
                currentState = CurrentStateType;
                signal = SignalType;
            }
        }

        public struct Transition
        {
            public Type? PreconditionType;
            public Type NewStateType;

            public Transition(Type? precondition, Type newState)
            {
                PreconditionType = precondition;
                NewStateType = newState;
            }

            public override bool Equals(object? obj)
            {
                return obj is Transition other &&
                        EqualityComparer<Type?>.Default.Equals(PreconditionType, other.PreconditionType) &&
                        EqualityComparer<Type>.Default.Equals(NewStateType, other.NewStateType);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(PreconditionType, NewStateType);
            }

            public void Deconstruct(out Type? precondition, out Type newState)
            {
                precondition = PreconditionType;
                newState = NewStateType;
            }
        }
    }
}
