using System;
using System.Linq;

namespace PSIBR.Liminality
{

    public class StateMachineBuilder
    {
        public StateBuilder StartsIn<TState>()
            where TState : class
        {
            return new StateBuilder(new StateMachineStateMap(typeof(TState)));
        }

        public static StateMachineStateMap BuildFromAttributes<TStateMachine>()
            where TStateMachine : StateMachine<TStateMachine>
        {
            var initialStateAttribute = typeof(TStateMachine).CustomAttributes.FirstOrDefault(a => a.AttributeType.IsGenericType && a.AttributeType.GetGenericTypeDefinition() == typeof(InitialStateAttribute<>));
            if (initialStateAttribute == null)
                throw new ArgumentException("State machines defined with attributes must have an InitialStateAttribute.");

            StateMachineStateMap stateMachineStateMap = new(initialStateAttribute.AttributeType.GenericTypeArguments[0]);

            var stateTypes = typeof(TStateMachine)
                .GetNestedTypes()
                .Select(t => new
                {
                    StateType = t,
                    Transitions = t.CustomAttributes
                        .Where(a => a.AttributeType.IsGenericType && a.AttributeType.GetGenericTypeDefinition() == typeof(TransitionAttribute<,>))
                })
                .Where(m => m.Transitions.Any());

            foreach (var stateType in stateTypes)
            {
                foreach (var transtion in stateType.Transitions)
                {
                    var args = transtion.AttributeType.GenericTypeArguments;
                    var signal = args[0];
                    var newState = args[1];

                    stateMachineStateMap[new StateMachineStateMap.Input(stateType.StateType, signal)] = new StateMachineStateMap.Transition(newState);
                }
            }

            return stateMachineStateMap;
        }

        public class StateBuilder
        {
            private readonly StateMachineStateMap _stateMachineStateMap;

            public StateBuilder(StateMachineStateMap stateMachineStateMap!!)
            {
                _stateMachineStateMap = stateMachineStateMap;
            }

            public ForStateContext<TState> For<TState>()
                where TState : class
                => new(this);

            public StateMachineStateMap Build() => _stateMachineStateMap;

            public StateBuilder AddTransition<TState, TSignal, TNewState>()
                where TState : class
                where TSignal : class, new()
                where TNewState : class
            {
                _stateMachineStateMap[new StateMachineStateMap.Input(typeof(TState), typeof(TSignal))] = new StateMachineStateMap.Transition(typeof(TNewState));

                return this;
            }
        }

        public class ForStateContext<TState>
            where TState : class
        {
            private readonly StateBuilder _stateBuilder;

            public ForStateContext(StateBuilder stateBuilder!!)
            {
                _stateBuilder = stateBuilder;
            }

            public OnContext<TState, TSignal> On<TSignal>()
                where TSignal : class, new()
                => new(_stateBuilder);
        }

        public class OnContext<TState, TSignal>
            where TState : class
            where TSignal : class, new()
        {
            private readonly StateBuilder _stateBuilder;

            public OnContext(StateBuilder stateBuilder!!)
            {
                _stateBuilder = stateBuilder;
            }

            public StateBuilder MoveTo<TNewState>()
                where TNewState : class
                => _stateBuilder.AddTransition<TState, TSignal, TNewState>();
        }
    }
}