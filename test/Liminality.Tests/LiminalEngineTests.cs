using Lamar;
using PSIBR.Liminality.Fixtures;
using PSIBR.Liminality.Tests;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PSIBR.Liminality
{
    public class LiminalEngineTests : IClassFixture<NoOpLiminalEngineFixture>
    {
        public LiminalEngineTests(NoOpLiminalEngineFixture fixture)
        {
            Fixture = fixture;
        }

        protected readonly NoOpLiminalEngineFixture Fixture;

        [Trait("Category", "Construction")]
        [Fact(DisplayName = "Constructing LiminalEngine with valid args succeeds")]
        public void LiminalEngine_Valid_Construction_Succeeds()
        {
            //Arrange
            var container = new Container(_ => { });

            //Act
            var liminalEngine = new LiminalEngine(container);

            //Assert
            //No assertion: Construction without exception is a pass
        }

        [Trait("Category", "Construction")]
        [Fact(DisplayName = "Constructing LiminalEngine with null args throws")]
        public void LiminalEngine_Invalid_Construction_Throws()
        {
            //Arrange
            IServiceProvider? services = default(IServiceProvider);

            //Act
            //Assert
            //CS8604 is intentionally disabled to get rid of compiler warnings
            //so that we can validate good handling of this condition for
            //consumers that don't have nullable enabled
#pragma warning disable CS8604 // Possible null reference argument.
            Assert.Throws<ArgumentNullException>(() => new LiminalEngine(services));
#pragma warning restore CS8604 // Possible null reference argument.
        }

        [Trait("Category", "Signals")]
        [Fact(DisplayName = "LiminalEngine SignalAsync with valid signal succeeds")]
        public async Task LiminalEngine_SignalAsync_Valid_Signal_Succeeds()
        {
            await Fixture.LiminalEngine.SignalAsync(
                Fixture.StateMachine,
                Fixture.Signal,
                _ => new ValueTask<object>(Fixture.Signal),
                (state, cancellationToken) => { state = Fixture.State; return new ValueTask(); }
            );
        }

        [Trait("Category", "Signals")]
        [Fact(DisplayName = "LiminalEngine SignalAsync with null signal succeeds")]
        public async Task LiminalEngine_SignalAsync_Invalid_Signal_Fails()
        {
            object? signal = null;
#pragma warning disable CS8604 // Possible null reference argument.
            //CS8604 is intentionally disabled to get rid of compiler warnings
            //so that we can validate good handling of this condition for
            //consumers that don't have nullable enabled

            await Fixture.LiminalEngine.SignalAsync<BasicStateMachine, object>(
                Fixture.StateMachine,
                signal,
                _ => new ValueTask<object>(Fixture.Signal),
                (state, cancellationToken) => { state = Fixture.State; return new ValueTask(); }
            );

#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}
