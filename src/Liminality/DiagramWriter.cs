using System.Text;

namespace PSIBR.Liminality
{
    public abstract class DiagramWriter<TStateMachine>
        where TStateMachine : StateMachine<TStateMachine>
    {
        public DiagramWriter(Graph<TStateMachine> graph!!)
        {
            Graph = graph;
        }

        protected readonly Graph<TStateMachine> Graph;
        public abstract Diagram Write();

        public abstract void WriteNode(Diagram diagram, GraphNode rootNode);
    }

    public abstract class Diagram
    {
        private StringBuilder _stringBuilder = new StringBuilder();
        public abstract void AddTransition(GraphNode? left, GraphNode? right);
        public virtual void AddSyntaxLine(string syntax)
        {
            _stringBuilder.AppendLine(syntax);
        }

        public virtual string Render()
        {
            return _stringBuilder.ToString();
        }
    }

    public class MermaidStateDiagramWriter<TStateMachine> : DiagramWriter<TStateMachine>
        where TStateMachine : StateMachine<TStateMachine>
    {
        public MermaidStateDiagramWriter(Graph<TStateMachine> graph) 
            : base(graph)
        {
        }

        public override Diagram Write()
        {
            var diagram = new MermaidStateDiagram();
            diagram.AddTransition(null, Graph);
            WriteNode(diagram, Graph);
            return diagram;
        }

        public override void WriteNode(Diagram diagram!!, GraphNode rootNode!!)
        {
            foreach (var node in rootNode)
            {
                diagram.AddTransition(rootNode, node);
                WriteNode(diagram, node);
            }
        }
    }
}
