using System;

namespace PSIBR.Liminality
{
    public class StateMachineBuilder<TStateMachine> 
        : StateMachineBuilder
    where TStateMachine : StateMachine<TStateMachine>
    {
        public StateMachineBuilder() 
            : base((Type initialState) => new StateMachineDefinition<TStateMachine>(initialState))
        {
        }
    }

    public class StateMachineBuilder
    {
        private readonly Func<Type, StateMachineDefinition> _definitionBuilder;

        public StateMachineBuilder(Func<Type, StateMachineDefinition> definitionBuilder)
        {
            _definitionBuilder = definitionBuilder;
        }

        public StateBuilder StartsIn<TState>()
            where TState : class
        {
            return new StateBuilder(_definitionBuilder(typeof(TState)));
        }

        public class StateBuilder
        {
            private readonly StateMachineDefinition _stateMachineDefintion;

            public StateBuilder(StateMachineDefinition stateMachineDefinition)
            {
                _stateMachineDefintion = stateMachineDefinition;
            }

            public ForStateContext<TState> For<TState>()
                where TState : class
                => new ForStateContext<TState>(this);

            public StateMachineDefinition Build() => _stateMachineDefintion;

            public StateBuilder AddTransition<TState, TSignal, TNewState>()
                where TState : class
                where TSignal : class, new()
                where TNewState : class
            {
                _stateMachineDefintion[new StateMachineDefinition.Input(typeof(TState), typeof(TSignal))] = new StateMachineDefinition.Transition(null, typeof(TNewState));

                return this;
            }

            public StateBuilder AddTransition<TState, TSignal, TPrecondition, TNewState>()
                where TState : class
                where TSignal : class, new()
                where TPrecondition : class
                where TNewState : class
            {
                _stateMachineDefintion[new StateMachineDefinition.Input(typeof(TState), typeof(TSignal))] = new StateMachineDefinition.Transition(typeof(TPrecondition), typeof(TNewState));

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

            public WhenContext<TState, TSignal, TPrecondition> When<TPrecondition>()
                where TPrecondition : class, IPrecondition<TSignal>
                => new WhenContext<TState, TSignal, TPrecondition>(_stateBuilder);
        }

        public class WhenContext<TState, TSignal, TPrecondition>
            where TState : class
            where TSignal : class, new()
            where TPrecondition : class, IPrecondition<TSignal>
        {
            private readonly StateBuilder _stateBuilder;

            public WhenContext(StateBuilder stateBuilder)
            {
                _stateBuilder = stateBuilder;
            }

            public StateBuilder MoveTo<TNewState>()
                where TNewState : class
                => _stateBuilder.AddTransition<TState, TSignal, TNewState>();
        }
    }
}