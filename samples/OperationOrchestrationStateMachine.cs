using Microsoft.Extensions.DependencyInjection;
using PSIBR.Liminality;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Samples
{
    using static Samples.OperationOrchestrationStateMachine;

    public static class OrchestrationStateMachineExtentions
    {
        public static void AddOperationOrchestration(this IServiceCollection services)
        {
            services.AddStateMachineDependenciesFromAttributes<OperationOrchestrationStateMachine>();

            services.AddScoped<Repository>();
        }
    }

    [InitialState<Created>]
    public class OperationOrchestrationStateMachine : StateMachine<OperationOrchestrationStateMachine>
    {
        public class Repository
        {
            private readonly LiminalEngine _engine;
            private readonly StateMachineDefinition<OperationOrchestrationStateMachine> _definition;

            public Repository(LiminalEngine engine, StateMachineDefinition<OperationOrchestrationStateMachine> definition)
            {
                _engine = engine;
                _definition = definition;
            }

            public OperationOrchestrationStateMachine Find(string orchestrationId) => new(_engine, _definition, orchestrationId);
        }

        public OperationOrchestrationStateMachine(
            LiminalEngine engine,
            StateMachineDefinition<OperationOrchestrationStateMachine> definition,
            string orchestrationId)
        : base(engine, definition)
        {
            if (engine is null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (string.IsNullOrWhiteSpace(orchestrationId))
            {
                throw new ArgumentException($"'{nameof(orchestrationId)}' cannot be null or whitespace.", nameof(orchestrationId));
            }

            OrchestrationId = orchestrationId;
        }

        protected object State { get; private set; } = new Created();

        public string OrchestrationId { get; }

        public async Task<AggregateSignalResult> SignalAsync<TSignal>(TSignal signal)
        where TSignal : class, new()
        {
            var valueTask = SignalAsync(
                signal,
                cancellationToken => new ValueTask<object>(State),
                (state, cancellationToken) => { State = state; return new ValueTask(); },
                default);

            return await valueTask.ConfigureAwait(false);
        }

        [Transition<Request, Requesting>]
        public class Created { }

        [Transition<Request.Acknowledgement, Requested>]
        public class Requesting
            : IAfterEnterHandler<OperationOrchestrationStateMachine, Request>
        {
            public async ValueTask<AggregateSignalResult?> AfterEnterAsync(SignalContext<OperationOrchestrationStateMachine> context, Request signal, CancellationToken cancellationToken = default)
            {
                //do work maybe publish to a queue and get a confirmation...

                return await context.Self.SignalAsync(new Request.Acknowledgement()).ConfigureAwait(false);
            }

            [Transition<Start, InProgress>]
            [Transition<Cancel, Cancelled>]
            public class Requested
                : IAfterEnterHandler<OperationOrchestrationStateMachine, Request.Acknowledgement>
            {
                public async ValueTask<AggregateSignalResult?> AfterEnterAsync(SignalContext<OperationOrchestrationStateMachine> context, Request.Acknowledgement signal, CancellationToken cancellationToken = default)
                {
                    //read from queue

                    return await context.Self.SignalAsync(new Start()).ConfigureAwait(false);
                }
            }
        }

        [Transition<Ping, InProgress>]
        [Transition<Pause, Pausing>]
        [Transition<Complete, Completed>]
        [Transition<Cancel, Cancelling>]
        [Transition<Throw, Failed>]
        public class InProgress
            : IAfterEnterHandler<OperationOrchestrationStateMachine, Start>
        {
            public ValueTask<AggregateSignalResult?> AfterEnterAsync(SignalContext<OperationOrchestrationStateMachine> context, Start signal, CancellationToken cancellationToken = default)
            {
                //do work

                // is the signal an internal signal of Requested (internal transition)?
                //var isSubState = signal.GetType().DeclaringType == typeof(Requested);



                return default;
            }
        }

        [Transition<Throw, Failed>]
        [Transition<Cancel, Cancelled>]
        public class Pausing { }

        [Transition<Resume, Requesting.Requested>]
        public class Paused { }

        [Transition<Throw, Failed>]
        [Transition<Cancel.Acknowledgement, Cancelled>]
        public class Cancelling { }

        public class Cancelled { }
        public class Completed { }
        public class Failed { }
        
        public class Request 
        {
            public class Acknowledgement { }
        }

        public class Cancel
        {
            public class Acknowledgement { }
        }
        public class Ping { }
        public class Resume { }
        public class Complete { }
        public class Throw { }
        public class Pause { }

        public class Start { }

    }
}
