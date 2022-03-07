using Lamar;
using PSIBR.Liminality.Tests;
using System;

namespace PSIBR.Liminality.Fixtures
{
    /// <summary>
    /// NoOpLiminalEngineFixture provides a set of instances for
    /// state machine components that can be used to validate
    /// the API surfaces, such as ensuring constructors and
    /// functions enforce based contract requirements
    /// </summary>
    public class NoOpLiminalEngineFixture : IDisposable
    {
        public class TestState { }

        public NoOpLiminalEngineFixture()
        {
            Container = new Container(_ => { });
            LiminalEngine = new LiminalEngine(Container);
            StateMachineStateMap = new StateMachineStateMap(typeof(object));
            StateMachineDefinition = new StateMachineDefinition<BasicStateMachine>(StateMachineStateMap);
            StateMachine = new BasicStateMachine(LiminalEngine, StateMachineDefinition);
            Signal = new object();
            State = new TestState();
        }

        public Container Container { get; private set; }
        public LiminalEngine LiminalEngine { get; private set; }
        public BasicStateMachine StateMachine { get; private set; }
        public StateMachineDefinition<BasicStateMachine> StateMachineDefinition { get; private set; }
        public StateMachineStateMap StateMachineStateMap { get; private set; }
        public object Signal { get; private set; }
        public TestState State { get; private set; }

        public void Dispose()
        {
            Container.Dispose();
        }
    }
}
