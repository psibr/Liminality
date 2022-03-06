using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public sealed class LiminalEngine
    {
        private readonly IServiceProvider _serviceProvider;
        public LiminalEngine(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private TransitionResolution<TStateMachine, TSignal>? ResolveTransition<TStateMachine, TSignal>(StateMachineStateMap stateMachineDefinition, Type stateType)
        where TStateMachine : StateMachine<TStateMachine>
        where TSignal : class, new()
        {
            if (!stateMachineDefinition.TryGetValue(new StateMachineStateMap.Input(stateType: stateType, signalType: typeof(TSignal)), out var transition))
                return default;

            var state = _serviceProvider.GetService(transition.NewStateType);

            IBeforeEnterHandler<TStateMachine, TSignal>? beforeEnterHandler = state as IBeforeEnterHandler<TStateMachine, TSignal>;

            if (state is null) throw new Exception($"Failed to resolve state: {transition.NewStateType.AssemblyQualifiedName}.");

            return new TransitionResolution<TStateMachine, TSignal>(transition, beforeEnterHandler, state);
        }

        public async ValueTask<AggregateSignalResult> SignalAsync<TStateMachine, TSignal>(
            TStateMachine self,
            TSignal signal,
            Func<CancellationToken, ValueTask<object>> loadStateFunc,
            Func<object, CancellationToken, ValueTask> persistStateFunc,
            CancellationToken cancellationToken = default)
        where TStateMachine : StateMachine<TStateMachine>
        where TSignal : class, new()
        {
            var loadStateValueTask = loadStateFunc(cancellationToken);
            if (!loadStateValueTask.IsCompletedSuccessfully) await loadStateValueTask.ConfigureAwait(false);

            object startingState = loadStateValueTask.Result;

            var resolution = ResolveTransition<TStateMachine, TSignal>(self.Definition.StateMap, startingState.GetType());

            if (resolution is null) return CreateResult(new TransitionNotFoundResult(startingState, signal));

            if (resolution.BeforeEnterHandler is not null)
            {
                ValueTask<AggregateException?> preconditionValueTask;

                try
                {
                    preconditionValueTask = resolution.BeforeEnterHandler.BeforeEnterAsync(new SignalContext<TStateMachine>(self, startingState, resolution.State), signal, cancellationToken);

                    if (preconditionValueTask.IsCompletedSuccessfully is false) await preconditionValueTask.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return CreateResult(new ExceptionThrownByPreconditionResult(startingState, signal, resolution.Transition, ex));
                }

                if (preconditionValueTask.Result is not null) return CreateResult(new RejectedByPreconditionResult(startingState, signal, resolution.Transition, preconditionValueTask.Result));
            }

            var persistStateValueTask = persistStateFunc(resolution.State, cancellationToken);
            if (!persistStateValueTask.IsCompletedSuccessfully) await persistStateValueTask.ConfigureAwait(false);

            if (resolution.Handler is null) return CreateResult(new TransitionedResult(startingState, resolution.State));

            ValueTask<AggregateSignalResult?> handlerValueTask;

            try
            {
                handlerValueTask = resolution.Handler.OnEnterAsync(
                    context: new SignalContext<TStateMachine>(self, startingState, resolution.State),
                    signal,
                    cancellationToken);

                if (!handlerValueTask.IsCompletedSuccessfully) await handlerValueTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return CreateResult(new ExceptionThrownByHandlerResult(startingState, signal, resolution.State, ex));
            }

            return CreateResult(new TransitionedResult(startingState, resolution.State), handlerValueTask.Result);

            AggregateSignalResult CreateResult(ISignalResult result, AggregateSignalResult? next = default)
            {
                var currentResult = new List<ISignalResult> { result };

                if (next is null) return new AggregateSignalResult(currentResult);

                return new AggregateSignalResult(next.Concat(currentResult).ToList());
            }
        }

        private sealed class TransitionResolution<TStateMachine, TSignal>
        where TStateMachine : StateMachine<TStateMachine>
        where TSignal : class, new()
        {
            public StateMachineStateMap.Transition Transition { get; }

            public IBeforeEnterHandler<TStateMachine, TSignal>? BeforeEnterHandler;

            public object State;

            public IOnEnterHandler<TStateMachine, TSignal>? Handler;

            public TransitionResolution(StateMachineStateMap.Transition transition, IBeforeEnterHandler<TStateMachine, TSignal>? precondition, object state)
            {
                Transition = transition;
                BeforeEnterHandler = precondition;
                State = state;
                Handler = state as IOnEnterHandler<TStateMachine, TSignal>;
            }
        }
    }
}
