using PSIBR.Liminality;

namespace Samples
{
    public partial class OperationOrchestrator
    {
        public class Repository
        {
            private readonly LiminalEngine _engine;
            private readonly StateMachineDefinition<OperationOrchestrator> _definition;

            public Repository(LiminalEngine engine, StateMachineDefinition<OperationOrchestrator> definition)
            {
                _engine = engine;
                _definition = definition;
            }

            public OperationOrchestrator Find(string orchestrationId) => new(_engine, _definition, orchestrationId);
        }

    }
}
