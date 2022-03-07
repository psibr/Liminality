using Lamar;
using PSIBR.Liminality.Fixtures;
using PSIBR.Liminality.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PSIBR.Liminality
{
    public class StateMachineTests : IClassFixture<NoOpLiminalEngineFixture>
    {
        public StateMachineTests(NoOpLiminalEngineFixture fixture)
        {
            Fixture = fixture;
        }

        protected readonly NoOpLiminalEngineFixture Fixture;

        [Trait("Category", "Construction")]
        [Fact(DisplayName = "Constructing LiminalEngine with valid args succeeds")]
        public void StateMachine_Valid_Construction_Succeeds()
        {
            //Arrange
            var container = new Container(_ => { });

            //Act
            //Though using BasicStateMachine, the goal is to validate
            //the base class construction
            var stateMachine = new BasicStateMachine(Fixture.LiminalEngine, Fixture.StateMachineDefinition);

            //Assert
            //No assertion: Construction without exception is a pass
        }

        [Trait("Category", "Construction")]
        [Fact(DisplayName = "Constructing LiminalEngine with null args throws")]
        public void StateMachine_Invalid_Construction_Throws()
        {
            //Arrange
            LiminalEngine? liminalEngine = default(LiminalEngine);
            StateMachineDefinition<BasicStateMachine>? stateMachineDefinition = default(StateMachineDefinition<BasicStateMachine>);

            //Act
            //Assert
            //CS8604 is intentionally disabled to get rid of compiler warnings
            //so that we can validate good handling of this condition for
            //consumers that don't have nullable enabled
#pragma warning disable CS8604 // Possible null reference argument.
            Assert.Throws<ArgumentNullException>(() => new BasicStateMachine(liminalEngine, stateMachineDefinition));
#pragma warning restore CS8604 // Possible null reference argument.
        }

        [Trait("Category", "Signals")]
        [Fact(DisplayName = "LiminalEngine SignalAsync with valid signal succeeds")]
        public async Task StateMachine_SignalAsync_Valid_Signal_Succeeds()
        {
            var stateMachine = new BasicStateMachine(Fixture.LiminalEngine, Fixture.StateMachineDefinition);
            await stateMachine.SignalAsync(
                Fixture.Signal,
                default
            );
        }
    }
}
