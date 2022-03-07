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
    public class StateMachineBuilderTests
    {
        [Trait("Category", "Construction")]
        [Fact(DisplayName = "Constructing StateMachineStateMap from attributes succeeds")]
        public void StateMachineBuilder_From_Attritbutes_Succeeds()
        {
            //Arrange

            //Act
            //Though using BasicStateMachine, the goal is to validate
            //the base class construction
            var stateMachineStateMap = StateMachineBuilder.BuildFromAttributes<BasicStateMachine>();

            //Assert
            //No assertion: Construction without exception is a pass
        }
    }
}
