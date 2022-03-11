using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PSIBR.Liminality;

namespace Samples.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OperationOrchestrationController : ControllerBase
    {
        private readonly ILogger<OperationOrchestrationController> _logger;

        public OperationOrchestrationController(
            ILogger<OperationOrchestrationController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(OperationOrchestrationStateMachine.Request request, [FromServices] OperationOrchestrationStateMachine.Repository operationOrchestrationRepository)
        {
            var operationOrchestrator = operationOrchestrationRepository.Find(Guid.NewGuid().ToString());

            var x = await operationOrchestrator.SignalAsync(request);

            return new JsonResult(x);
        }
    }
}
