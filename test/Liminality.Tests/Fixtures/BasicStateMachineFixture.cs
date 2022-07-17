using Lamar;
using Microsoft.Extensions.DependencyInjection;
using System;
using static PSIBR.Liminality.Tests.BasicStateMachine;

namespace PSIBR.Liminality.Tests.Fixtures
{
    public class BasicStateMachineFixture : IDisposable
    {
        public BasicStateMachineFixture()
        {
            var container = new Container(x =>
            {
                x.AddStateMachineDependencies<BasicStateMachine>(builder => builder
                    .StartsIn<Idle>()
                    .For<Idle>().On<Start>().MoveTo<InProgress>()
                    .For<InProgress>().On<Finish>().MoveTo<Finished>()
                    .For<InProgress>().On<Cancel>().MoveTo<Idle>()
                    .Build());

                x.AddTransient<BasicStateMachine>();
            });

            BasicStateMachine = container.GetService<BasicStateMachine>() ?? throw new Exception("Container not properly setup");
        }

        public BasicStateMachine BasicStateMachine { get; }

        public void Dispose()
        {
        }
    }
}
