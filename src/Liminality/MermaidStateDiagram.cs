namespace PSIBR.Liminality
{
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
