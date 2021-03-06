﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public sealed class Engine<TStateMachine>
    where TStateMachine : StateMachine<TStateMachine>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly StateMachineDefinition _stateMachineDefinition;

        public Engine(IServiceProvider serviceProvider, StateMachineDefinition<TStateMachine> stateMachineDefinition)
        {
            _serviceProvider = serviceProvider;
            _stateMachineDefinition = stateMachineDefinition;
        }

        private TransitionResolution<TSignal>? ResolveTransition<TSignal>(Type stateType)
        where TSignal : class, new()
        {
            if (!_stateMachineDefinition.TryGetValue(new StateMachineDefinition.Input(stateType: stateType, signalType: typeof(TSignal)), out var transition))
                return default;

            IPrecondition<TSignal>? precondition = !(transition.PreconditionType is null)
                ? _serviceProvider.GetService(transition.PreconditionType) as IPrecondition<TSignal>
                : null;

            var state = _serviceProvider.GetService(transition.NewStateType);

            if (state is null) throw new Exception($"Failed to resolve state: {transition.NewStateType.AssemblyQualifiedName}.");

            return new TransitionResolution<TSignal>(transition, precondition, state);
        }

        public async ValueTask<AggregateSignalResult> SignalAsync<TSignal>(
            TStateMachine self,
            TSignal signal,
            Func<CancellationToken, ValueTask<object>> loadStateFunc,
            Func<object, CancellationToken, ValueTask> persistStateFunc,
            CancellationToken cancellationToken = default)
        where TSignal : class, new()
        {
            var loadStateValueTask = loadStateFunc(cancellationToken);
            if (!loadStateValueTask.IsCompletedSuccessfully) await loadStateValueTask.ConfigureAwait(false);

            object startingState = loadStateValueTask.Result;

            var resolution = ResolveTransition<TSignal>(startingState.GetType());

            if (resolution is null) return CreateResult(new TransitionNotFoundResult(startingState, signal));

            if (!(resolution.Precondition is null))
            {
                ValueTask<AggregateException?> preconditionValueTask;

                try
                {
                    preconditionValueTask = resolution.Precondition.CheckAsync(signal, cancellationToken);

                    if (!preconditionValueTask.IsCompletedSuccessfully) await preconditionValueTask.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return CreateResult(new ExceptionThrownByPreconditionResult(startingState, signal, resolution.Transition, ex));
                }

                if (!(preconditionValueTask.Result is null)) return CreateResult(new RejectedByPreconditionResult(startingState, signal, resolution.Transition, preconditionValueTask.Result));
            }

            var persistStateValueTask = persistStateFunc(resolution.State, cancellationToken);
            if (!persistStateValueTask.IsCompletedSuccessfully) await persistStateValueTask.ConfigureAwait(false);

            if (resolution.Handler is null) return CreateResult(new TransitionedResult(startingState, resolution.State));

            ValueTask<AggregateSignalResult?> handlerValueTask;

            try
            {
                handlerValueTask = resolution.Handler.InvokeAsync(
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

        private sealed class TransitionResolution<TSignal>
        where TSignal : class, new()
        {
            public StateMachineDefinition.Transition Transition { get; }

            public IPrecondition<TSignal>? Precondition;

            public object State;

            public ISignalHandler<TStateMachine, TSignal>? Handler;

            public TransitionResolution(StateMachineDefinition.Transition transition, IPrecondition<TSignal>? precondition, object state)
            {
                Transition = transition;
                Precondition = precondition;
                State = state;
                Handler = state as ISignalHandler<TStateMachine, TSignal>;
            }
        }
    }
}
