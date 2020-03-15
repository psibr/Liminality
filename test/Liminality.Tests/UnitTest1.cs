using System;
using System.Collections.Generic;
using Xunit;

namespace PSIBR.Liminality.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
        }
    }

    // States
    public class Idle { }
    public class InProgress { }
    public class Finished { }

    // Inputs
    public class Start { }
    public class Finish { }
}
