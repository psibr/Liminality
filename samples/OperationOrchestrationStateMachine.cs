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
            services.AddStateMachineDependenciesFromAttributes<OperationOrchestrationStateMachine>();

            services.AddSingleton<Repository>();
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

        [Transition<Request, Requesting>]
        public class Created { }

        [Transition<Request.Acknowledgement, Requested>]
        public class Requesting { }

        [Transition<Start, InProgress>]
        [Transition<Cancel, Cancelled>]
        public class Requested { }

        [Transition<Ping, InProgress>]
        [Transition<Pause, Pausing>]
        [Transition<Complete, Completed>]
        [Transition<Cancel, Cancelling>]
        [Transition<Throw, Failed>]
        public class InProgress { }

        [Transition<Throw, Failed>]
        [Transition<Cancel, Cancelled>]
        public class Pausing { }

        [Transition<Resume, Requested>]
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
