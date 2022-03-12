using PSIBR.Liminality;

namespace Samples
{
    public partial class OperationOrchestrator
    {
        [Transition<Throw, Failed>]
        [Transition<Cancel.Acknowledgement, Cancelled>]
        public class Cancelling { }

    }
}
