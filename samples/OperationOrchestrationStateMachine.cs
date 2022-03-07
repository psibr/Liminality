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

    [IntialState<Created>]
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

        [TransitionAttribute<Request, Requesting>]
        public class Created { }

        [TransitionAttribute<Request.Acknowledgement, Requested>]
        public class Requesting { }

        [TransitionAttribute<Start, InProgress>]
        [TransitionAttribute<Cancel, Cancelled>]
        public class Requested { }

        [TransitionAttribute<Ping, InProgress>]
        [TransitionAttribute<Pause, Pausing>]
        [TransitionAttribute<Complete, Completed>]
        [TransitionAttribute<Cancel, Cancelling>]
        [TransitionAttribute<Throw, Failed>]
        public class InProgress { }

        [TransitionAttribute<Throw, Failed>]
        [TransitionAttribute<Cancel, Cancelled>]
        public class Pausing { }

        [TransitionAttribute<Resume, Requested>]
        public class Paused { }

        [TransitionAttribute<Throw, Failed>]
        [TransitionAttribute<Cancel.Acknowledgement, Cancelled>]
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
