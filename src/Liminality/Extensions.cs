namespace PSIBR.Liminality
{
    public static class Extensions
    {
        public static Graph<TStateMachine> GetGraph<TStateMachine>(this TStateMachine stateMachine!!)
            where TStateMachine : StateMachine<TStateMachine>
        {
            var builder = new GraphBuilder<TStateMachine>(stateMachine);
            return builder.Build();
        }

        public static Diagram GetDiagram<TStateMachine>(this Graph<TStateMachine> graph!!)
            where TStateMachine : StateMachine<TStateMachine>
        {
            var writer = new MermaidStateDiagramWriter<TStateMachine>(graph);
            return writer.Write();
        }
    }
}
