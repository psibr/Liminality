using PSIBR.Liminality;

namespace Samples
{
    public partial class OperationOrchestrator
    {
        [Transition<Throw, Failed>]
        [Transition<Cancel, Cancelled>]
        public class Pausing { }

    }
}
