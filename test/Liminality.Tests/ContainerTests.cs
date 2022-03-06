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

    public class ContainerTests
    {
        [Fact]
        public void CanResolveStateMachine()
        {
            var container = new Container(x =>
            {
                x.AddStateMachineDependencies<BasicStateMachine>(builder => builder
                    .StartsIn<Idle>()
                    .For<Idle>().On<Start>().MoveTo<InProgress>()
                    .For<InProgress>().On<Finish>().MoveTo<Finished>()
                    .Build());

                x.AddTransient<BasicStateMachine>();
            });

            var engine = container.GetService<BasicStateMachine>();

            Assert.NotNull(engine);
        }

        [Fact]
        public void CanResolveStateMachineFromFactory()
        {
            var container = new Container(x =>
            {
                x.AddStateMachineDependencies<BasicStateMachine>(builder => builder
                    .StartsIn<Idle>()
                    .For<Idle>().On<Start>().MoveTo<InProgress>()
                    .For<InProgress>().On<Finish>().MoveTo<Finished>()
                    .Build());

                x.AddScoped<Factory>();
            });

            var factory = container.GetRequiredService<Factory>();

            var basicStateMachine = factory.Create();

            Assert.NotNull(basicStateMachine);
        }
    }

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
        public class Idle { }
        public class InProgress { }
        public class Finished { }

        // Inputs
        public class Start { }
        public class Finish { }
    }
}
