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

            var engine = container.GetService<LiminalEngine>();

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
            });

            var basicStateMachine = container.GetRequiredService<BasicStateMachine>();

            Assert.NotNull(basicStateMachine);
        }
    }

    public class BasicStateMachine : StateMachine<BasicStateMachine>
    {
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
