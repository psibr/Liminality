using System;
using System.Collections.Generic;

namespace PSIBR.Liminality
{
    using TransitionMap = Dictionary<(Type CurrentState, Type Signal), (Type? Precondition, Type NewState)>;

    public class StateMachineBuilder
    {
        private Type? _initialState;
        protected readonly TransitionMap TransitionMap = new TransitionMap();
        protected readonly StateBuilder _stateBuilder;

        public StateMachineBuilder()
        {
            _stateBuilder = new StateBuilder(this);
        }

        public StateBuilder StartsIn<TState>()
            where TState : class
        {
            _initialState = typeof(TState);

            return _stateBuilder;
        }

        private StateBuilder AddTransition<TState, TSignal, TNewState>()
            where TState : class
            where TSignal : class, new()
            where TNewState : class
        {
            TransitionMap[(typeof(TState), typeof(TSignal))] = (null, typeof(TNewState));

            return _stateBuilder;
        }

        private StateBuilder AddTransition<TState, TSignal, TPrecondition, TNewState>()
            where TState : class
            where TSignal : class, new()
            where TPrecondition : class
            where TNewState : class
        {
            TransitionMap[(typeof(TState), typeof(TSignal))] = (typeof(TPrecondition), typeof(TNewState));

            return _stateBuilder;
        }

        public class StateBuilder
        {
            private readonly StateMachineBuilder _stateMachineBuilder;

            public StateBuilder(StateMachineBuilder stateMachineBuilder)
            {
                _stateMachineBuilder = stateMachineBuilder;
            }

            public ForStateContext<TState> For<TState>()
                where TState : class
                => new ForStateContext<TState>(_stateMachineBuilder);

            public TransitionMap Build() => _stateMachineBuilder.TransitionMap;
        }

        public class ForStateContext<TState>
            where TState : class
        {
            private readonly StateMachineBuilder _stateMachineBuilder;

            public ForStateContext(StateMachineBuilder stateMachineBuilder)
            {
                _stateMachineBuilder = stateMachineBuilder;
            }

            public OnContext<TState, TSignal> On<TSignal>()
                where TSignal : class, new()
                => new OnContext<TState, TSignal>(_stateMachineBuilder);
        }

        public class OnContext<TState, TSignal>
            where TState : class
            where TSignal : class, new()
        {
            private readonly StateMachineBuilder _stateMachineBuilder;

            public OnContext(StateMachineBuilder stateMachineBuilder)
            {
                _stateMachineBuilder = stateMachineBuilder;
            }

            public StateBuilder MoveTo<TNewState>()
                where TNewState : class
                => _stateMachineBuilder.AddTransition<TState, TSignal, TNewState>();

            public WhenContext<TState, TSignal, TPrecondition> When<TPrecondition>()
                where TPrecondition : class, IPrecondition<TSignal>
                => new WhenContext<TState, TSignal, TPrecondition>(_stateMachineBuilder);
        }

        public class WhenContext<TState, TSignal, TPrecondition>
            where TState : class
            where TSignal : class, new()
            where TPrecondition : class, IPrecondition<TSignal>
        {
            private readonly StateMachineBuilder _stateMachineBuilder;

            public WhenContext(StateMachineBuilder stateMachineBuilder)
            {
                _stateMachineBuilder = stateMachineBuilder;
            }

            public StateBuilder MoveTo<TNewState>()
                where TNewState : class
                => _stateMachineBuilder.AddTransition<TState, TSignal, TNewState>();
        }
    }


}