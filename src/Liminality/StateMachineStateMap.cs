using System;
using System.Collections.Generic;

namespace PSIBR.Liminality
{
    public class StateMachineStateMap : Dictionary<StateMachineStateMap.Input, StateMachineStateMap.Transition>
    {
        public StateMachineStateMap(Type initialState)
        {
            InitialState = initialState;
        }

        public Type InitialState { get; }

        public record Input
        {
            public Type CurrentStateType;
            public Type SignalType;

            public Input(Type stateType, Type signalType)
            {
                CurrentStateType = stateType;
                SignalType = signalType;
            }

            public void Deconstruct(out Type currentState, out Type signal)
            {
                currentState = CurrentStateType;
                signal = SignalType;
            }
        }

        public record Transition
        {
            public Type NewStateType;

            public Transition(Type newState)
            {
                NewStateType = newState;
            }

            public void Deconstruct(out Type newState)
            {
                newState = NewStateType;
            }
        }
    }
}
