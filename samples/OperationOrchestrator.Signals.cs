namespace Samples
{
    public partial class OperationOrchestrator
    {
        public class Request
        {
            public class Acknowledgement { }
        }

        public class Cancel
        {
            public class Acknowledgement { }
        }
        public class Ping { }
        public class Resume { }
        public class Complete { }
        public class Throw { }
        public class Pause { }

        public class Start { }
    }
}
