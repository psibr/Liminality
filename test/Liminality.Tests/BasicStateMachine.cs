using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality.Tests
{
    [InitialState<Idle>]
    public class BasicStateMachine : StateMachine<BasicStateMachine>
    {
        public class Factory
        {
            private readonly LiminalEngine _engine;
            private readonly StateMachineDefinition<BasicStateMachine> _definition;

            public Factory(LiminalEngine engine, StateMachineDefinition<BasicStateMachine> definition)
            {
                _engine = engine;
                _definition = definition;
            }

            public BasicStateMachine Create() => new(_engine, _definition);
        }

        public BasicStateMachine(LiminalEngine engine, StateMachineDefinition<BasicStateMachine> definition) : base(engine,definition)
        {
        }

        private object State { get; set; } = new Idle();

        public ValueTask<AggregateSignalResult> SignalAsync<TSignal>(TSignal signal, CancellationToken cancellationToken)
            where TSignal : class, new()
        {
            return base.SignalAsync(signal, _ => new ValueTask<object>(State), (state, _) => { State = state; return new ValueTask(); }, cancellationToken);
        }

        // States
        [Transition<Start, InProgress>]
        public class Idle { }

        [Transition<Finish, Finished>]
        public class InProgress { }

        public class Finished { }

        // Inputs
        public class Start { }
        public class Finish { }
        public class Cancel { }
    }
}