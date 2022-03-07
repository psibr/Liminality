using System;
using System.Collections.Generic;
using Xunit;
using Lamar;
using PSIBR.Liminality;
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
}
