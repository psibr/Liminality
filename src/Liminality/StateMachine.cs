using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public abstract class StateMachine<TStateMachine>
    where TStateMachine : StateMachine<TStateMachine>
    {
        private readonly Resolver<TStateMachine> _resolver;

        protected StateMachine(Resolver<TStateMachine> resolver)
        {
            _resolver = resolver;
        }

        protected async ValueTask<ISignalResult> SignalAsync<TSignal>(
            TSignal signal,
            Func<CancellationToken, ValueTask<object>> loadStateFunc,
            Func<object, CancellationToken, ValueTask> persistStateFunc,
            CancellationToken cancellationToken = default)
        where TSignal : class, new()
        {
            var loadStateValueTask = loadStateFunc(cancellationToken);
            if(!loadStateValueTask.IsCompletedSuccessfully) await loadStateValueTask.ConfigureAwait(false);

            var startingState = loadStateValueTask.Result;

            TransitionResolution<TStateMachine, TSignal>? resolution = _resolver.ResolveTransition<TSignal>(startingState.GetType());

            if (resolution is null) return new TransitionNotFoundResult(startingState, signal);

            if (!(resolution.Precondition is null))
            {
                var preconditionValueTask = resolution.Precondition.CheckAsync(signal, cancellationToken);
                // TODO: should catch any exceptions here
                if (!preconditionValueTask.IsCompletedSuccessfully) await preconditionValueTask.ConfigureAwait(false);

                if (!(preconditionValueTask.Result is null)) return new RejectedByPreconditionResult(startingState, signal, resolution.Transition, preconditionValueTask.Result);
            }

            var persistStateValueTask = persistStateFunc(resolution.State, cancellationToken);
            if(!persistStateValueTask.IsCompletedSuccessfully) await persistStateValueTask.ConfigureAwait(false);

            if (resolution.Handler is null) return new TransitionedResult(startingState, resolution.State);
            
            var handlerValueTask = resolution.Handler.InvokeAsync(new SignalContext<TStateMachine>((TStateMachine)this, startingState, resolution.State), signal, cancellationToken);

            // TODO: should catch any exceptions here
            if (!handlerValueTask.IsCompletedSuccessfully) await handlerValueTask.ConfigureAwait(false);

            // replace with a multiple transition result?
            return new TransitionedResult(startingState, resolution.State);
        }
    }
}
