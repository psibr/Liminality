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

    public class MermaidDiagramWriter<TStateMachine> : DiagramWriter<TStateMachine>
        where TStateMachine : StateMachine<TStateMachine>
    {
        public MermaidDiagramWriter(Graph<TStateMachine> graph) 
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

    public class MermaidStateDiagram : Diagram
    {
        private const string DiagramTypeToken = "stateDiagram-v2";
        private const string Indent = "    ";
        private const string GoesToToken = "-->";
        private const string InitialStateToken = "[*]";

        public MermaidStateDiagram()
        {
            AddSyntaxLine($"{DiagramTypeToken}");
        }

        public override void AddTransition(GraphNode? left, GraphNode? right)
        {
            string leftSyntax = left is null ? InitialStateToken : left.Name;
            string rightSyntax = right is null ? InitialStateToken : right.Name;
            string signalSyntax = left?.Condition is not null ? $":{left.Condition}" : string.Empty;

            if (leftSyntax != rightSyntax && !string.IsNullOrWhiteSpace(signalSyntax))
            {
                var syntax = $"{Indent}{leftSyntax} {GoesToToken} {rightSyntax}{signalSyntax}";
                AddSyntaxLine(syntax);
            }
        }
    }
}
