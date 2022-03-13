using PSIBR.Liminality.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PSIBR.Liminality
{
    public class VisualizationTests : IClassFixture<BasicStateMachineFixture>
    {
        public VisualizationTests(BasicStateMachineFixture fixture)
        {
            Fixture = fixture;
        }

        protected readonly BasicStateMachineFixture Fixture;

        [Fact]
        public void Creating_Graph_Succeeds()
        {
            Fixture.BasicStateMachine.GetGraph();
        }

        [Fact]
        public void Creating_Diagram_Succeeds()
        {
            var graph = Fixture.BasicStateMachine.GetGraph();
            Assert.NotNull(graph);
            var diagram = graph.GetDiagram();
            Assert.NotNull(diagram);
            var render = diagram.Render();
            Assert.NotNull(render);
        }
    }
}
