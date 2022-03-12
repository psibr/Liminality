using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PSIBR.Liminality
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class PossibleSignalAttribute<TSignal>
        : Attribute
    where TSignal : class, new()
    { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TransitionAttribute<TSignal, TState>
        : Attribute
    where TSignal : class, new()
    where TState : class, new()
    { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class InitialStateAttribute<TState>
        : Attribute
    where TState : class, new()
    { }

    public class StateMachineBuilder
    {
        public StateBuilder StartsIn<TState>()
            where TState : class
        {
            return new StateBuilder(new StateMachineStateMap(typeof(TState)));
        }

        internal record TransitionMatches(Type StateType, IEnumerable<CustomAttributeData> Transitions);

        public static StateMachineStateMap BuildFromAttributes<TStateMachine>()
            where TStateMachine : StateMachine<TStateMachine>
        {
            var initialStateAttribute = typeof(TStateMachine).CustomAttributes.FirstOrDefault(a => a.AttributeType.IsGenericType && a.AttributeType.GetGenericTypeDefinition() == typeof(InitialStateAttribute<>));
            if (initialStateAttribute == null)
                throw new ArgumentException("State machines defined with attributes must have an InitialStateAttribute.");

            StateMachineStateMap stateMachineStateMap = new(initialStateAttribute.AttributeType.GenericTypeArguments[0]);

            Dictionary<int, IEnumerable<TransitionMatches>> matches = new();
            int index = 0;
            IEnumerable<TransitionMatches> currentLevel =  new[] { new TransitionMatches(typeof(TStateMachine), Enumerable.Empty<CustomAttributeData>()) };
            do
            {
                matches[index] = DiscoverPotentialTransitionStates(currentLevel);
                index++;
                currentLevel = matches[index - 1];

            } while (matches[index-1].Any());

            foreach (var match in matches.SelectMany(s => s.Value))
            {
                if (match.Transitions.Any())
                {
                    foreach (var transtion in match.Transitions)
                    {
                        var args = transtion.AttributeType.GenericTypeArguments;
                        var signal = args[0];
                        var newState = args[1];

                        stateMachineStateMap[new StateMachineStateMap.Input(match.StateType, signal)] = new StateMachineStateMap.Transition(newState);
                    }
                }
            }

            return stateMachineStateMap;

            static IEnumerable<TransitionMatches> DiscoverPotentialTransitionStates(IEnumerable<TransitionMatches> matches)
            {
                return matches
                    .Select(match => match.StateType
                        .GetNestedTypes()
                        .Select(t => new TransitionMatches(StateType: t, Transitions: t.CustomAttributes
                                .Where(a => a.AttributeType.IsGenericType && a.AttributeType.GetGenericTypeDefinition() == typeof(TransitionAttribute<,>))
                    ))).SelectMany(types => types);
            }
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