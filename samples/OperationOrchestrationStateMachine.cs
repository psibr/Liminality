using Microsoft.Extensions.DependencyInjection;
using PSIBR.Liminality;
using System;
using System.Threading.Tasks;

namespace Samples
{
    using static Samples.OperationOrchestrationStateMachine;

    public static class OrchestrationStateMachineExtentions
    {
        public static void AddOperationOrchestrationStateMachine(this IServiceCollection services)
        {
            services.AddStateMachineDependencies<OperationOrchestrationStateMachine>(builder => builder
                .StartsIn<Created>()

                .For<Created>().On<Request>().MoveTo<Requesting>()

                .For<Requesting>().On<Request.Acknowledgement>().MoveTo<Requested>()
                .For<Requesting>().On<Reset>().MoveTo<Created>()

                .For<Requested>().On<Starting>().MoveTo<InProgress>()
                .For<Requested>().On<Cancel>().MoveTo<Created>()
                .For<Requested>().On<Reset>().MoveTo<Failed>()


                .For<InProgress>().On<Ping>().MoveTo<InProgress>()
                .For<InProgress>().On<Complete>().MoveTo<Created>()
                .For<InProgress>().On<Pause>().MoveTo<Pausing>()
                .For<InProgress>().On<Throw>().MoveTo<Failed>()
                .For<InProgress>().On<Cancel>().MoveTo<Cancelling>()

                .For<Paused>().On<Resume>().MoveTo<Requested>()

                .For<Pausing>().On<Throw>().MoveTo<Failed>()
                .For<Pausing>().On<Cancel>().MoveTo<Cancelled>()

                // Handle duplicates idempotently
                .For<Cancelling>().On<Cancel>().MoveTo<Cancelling>()
                .For<Cancelling>().On<Throw>().MoveTo<Failed>()

                .For<Cancelling>().On<Cancel.Acknowledgement>().MoveTo<Cancelled>()
                .Build());

            services.AddSingleton<Repository>();
        }
    }

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

        public AggregateSignalResult Signal<TSignal>(TSignal signal)
        where TSignal : class, new()
        {
            var valueTask = SignalAsync(
                signal,
                cancellationToken => new ValueTask<object>(State),
                (state, cancellationToken) => { State = state; return new ValueTask(); },
                default);

            // handle synchronyously ONLY as an example of this capability
            if (valueTask.IsCompletedSuccessfully) return valueTask.Result;
            else if (valueTask.IsFaulted && valueTask.AsTask().Exception is Exception ex) throw ex;
            else throw new NotSupportedException("This state machine cannot execute async");
        }

        public class Created { }
        public class Requesting { }
        public class Requested { }
        public class Starting { }
        public class InProgress { }
        public class Pausing { }
        public class Paused { }
        public class Cancelling { }
        public class Cancelled { }
        public class Succeeded { }
        public class Failed { }
        
        public class Request 
        {
            public class Acknowledgement { }
        }
        public class Reset { }
        public class Cancel
        {
            public class Acknowledgement { }
        }
        public class Ping { }
        public class Resume { }
        public class Complete { }
        public class Throw { }
        public class Pause { }

    }
}
