using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PSIBR.Liminality;

namespace Samples.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OperationOrchestratorController : ControllerBase
    {
        private readonly ILogger<OperationOrchestratorController> _logger;

        public OperationOrchestratorController(
            ILogger<OperationOrchestratorController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(OperationOrchestrator.Request request, [FromServices] OperationOrchestrator.Repository operationOrchestrationRepository)
        {
            var operationOrchestrator = operationOrchestrationRepository.Find(Guid.NewGuid().ToString());

            var requestResult = await operationOrchestrator.SignalAsync(request);

            if (requestResult.InnerResult is not TransitionedResult transitionResult
                || transitionResult.NewState is not OperationOrchestrator.Requesting.Requested)

            {
                return Problem(statusCode: 500);
            }

            var startResult = await operationOrchestrator.SignalAsync(new OperationOrchestrator.Start());

            if (startResult.InnerResult is not TransitionedResult transitionResult2
                || transitionResult2.NewState is not OperationOrchestrator.InProgress)
            {
                return Problem(statusCode: 500);
            }

            return Ok(AggregateSignalResult.Combine(requestResult, startResult));
        }
    }
}
