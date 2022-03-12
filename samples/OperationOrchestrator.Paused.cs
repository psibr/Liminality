using PSIBR.Liminality;

namespace Samples
{
    public partial class OperationOrchestrator
    {
        [Transition<Resume, Requesting.Requested>]
        public class Paused { }

    }
}
