using System;

namespace PSIBR.Liminality
{

    public class StateMachineBuilder
    {
        public StateBuilder StartsIn<TState>()
            where TState : class
        {
            return new StateBuilder(new StateMachineStateMap(typeof(TState)));
        }

        public class StateBuilder
        {
            private readonly StateMachineStateMap _stateMachineDefintion;

            public StateBuilder(StateMachineStateMap stateMachineDefinition)
            {
                _stateMachineDefintion = stateMachineDefinition;
            }

            public ForStateContext<TState> For<TState>()
                where TState : class
                => new ForStateContext<TState>(this);

            public StateMachineStateMap Build() => _stateMachineDefintion;

            public StateBuilder AddTransition<TState, TSignal, TNewState>()
                where TState : class
                where TSignal : class, new()
                where TNewState : class
            {
                _stateMachineDefintion[new StateMachineStateMap.Input(typeof(TState), typeof(TSignal))] = new StateMachineStateMap.Transition(typeof(TNewState));

                return this;
            }
        }

        public class ForStateContext<TState>
            where TState : class
        {
            private readonly StateBuilder _stateBuilder;

            public ForStateContext(StateBuilder stateBuilder)
            {
                _stateBuilder = stateBuilder;
            }

            public OnContext<TState, TSignal> On<TSignal>()
                where TSignal : class, new()
                => new OnContext<TState, TSignal>(_stateBuilder);
        }

        public class OnContext<TState, TSignal>
            where TState : class
            where TSignal : class, new()
        {
            private readonly StateBuilder _stateBuilder;

            public OnContext(StateBuilder stateBuilder)
            {
                _stateBuilder = stateBuilder;
            }

            public StateBuilder MoveTo<TNewState>()
                where TNewState : class
                => _stateBuilder.AddTransition<TState, TSignal, TNewState>();
        }
    }
}