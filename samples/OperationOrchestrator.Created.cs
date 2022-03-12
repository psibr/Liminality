using PSIBR.Liminality;

namespace Samples
{
    public partial class OperationOrchestrator
    {
        [Transition<Request, Requesting>]
        public class Created { }

    }
}
