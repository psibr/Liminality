using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PSIBR.Liminality;

namespace Samples.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReverseStringAsExtraStateController : ControllerBase
    {
        private readonly ILogger<ReverseStringAsExtraStateController> _logger;

        public ReverseStringAsExtraStateController(
            ILogger<ReverseStringAsExtraStateController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(ReverseStringAsExtraState.LoadValues values, [FromServices] ReverseStringAsExtraState stateMachine)
        {
            // load the input values (Idle (current state), LoadValues (signal) => Idle (new state))
            await stateMachine.SignalAsync(values);

            //kick off the machine's processing
            await stateMachine.SignalAsync(new ReverseStringAsExtraState.StartProcessing());

            // in this example state was stored on the machine instance
            return Ok(stateMachine.Output);
        }
    }
}
