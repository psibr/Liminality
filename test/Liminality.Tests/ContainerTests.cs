using System;
using System.Collections.Generic;
using Xunit;
using Lamar;
using PSIBR.Liminality;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace PSIBR.Liminality.Tests
{
    using static BasicStateMachine;

    public class UnitTest1
    {
        [Fact]
        public void CanResolveEngine()
        {
            var container = new Container(x =>
            {
                x.AddStateMachine<BasicStateMachine>(builder => builder
                    .StartsIn<Idle>()
                    .For<Idle>().On<Start>().MoveTo<InProgress>()
                    .For<InProgress>().On<Finish>().MoveTo<Finished>()
                    .Build());
            });

            var engine = container.GetService(typeof(Engine<BasicStateMachine>)) as Engine<BasicStateMachine>;

            Assert.NotNull(engine);
        }

        [Fact]
        public void CanResolveEngineFromFactory()
        {
            var container = new Container(x =>
            {
                x.AddStateMachine<BasicStateMachine>(builder => builder
                    .StartsIn<Idle>()
                    .For<Idle>().On<Start>().MoveTo<InProgress>()
                    .For<InProgress>().On<Finish>().MoveTo<Finished>()
                    .Build());

                x.AddScoped<Factory>();
            });

            var factory = container.GetRequiredService<Factory>();

            var stateMachine = factory.Create();

            Assert.NotNull(stateMachine);
        }
    }

    public class BasicStateMachine : StateMachine<BasicStateMachine>
    {
        public BasicStateMachine(Engine<BasicStateMachine> engine) : base(engine)
        {
        }

        private object State { get; set; } = new Idle();

        public ValueTask<AggregateSignalResult> SignalAsync<TSignal>(TSignal signal, CancellationToken cancellationToken)
            where TSignal : class, new()
        {
            return base.SignalAsync(signal, _ => new ValueTask<object>(State), (state, _) => { State = state; return new ValueTask(); }, cancellationToken);
        }

        public class Factory
        {
            private readonly Engine<BasicStateMachine> _engine;

            public Factory(Engine<BasicStateMachine> engine)
            {
                _engine = engine;
            }

            public BasicStateMachine Create() => new BasicStateMachine(_engine);
        }

        // States
        public class Idle { }
        public class InProgress { }
        public class Finished { }

        // Inputs
        public class Start { }
        public class Finish { }
    }
}
