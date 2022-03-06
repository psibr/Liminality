using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PSIBR.Liminality;

namespace Samples.Controllers
{
    [ApiController]
    [Route("assays/[controller]")]
    public class Covid19Controller : ControllerBase
    {
        private readonly ILogger<Covid19Controller> _logger;

        public Covid19Controller(
            ILogger<Covid19Controller> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post(Covid19TestKit.BiologicalSequenceSample sample, [FromServices] Covid19TestKit.Factory testKitFactory)
        {
            Covid19TestKit covid19Assay = testKitFactory.CreateKit();
            
            var result = covid19Assay.Signal(sample);

            return result.InnerResult switch
            {
                TransitionedResult transitioned => transitioned.NewState switch
                {
                    Covid19TestKit.Positive _ => Ok(1),
                    Covid19TestKit.Negative _ => Ok(-1),
                    Covid19TestKit.Inconclusive _ => Ok(0),
                    _ => throw new Exception("Assesment ended prematurely")
                },
                ExceptionThrownByBeforeEnterHandlerResult rejectionOrFailure => rejectionOrFailure.Exceptions.InnerException switch
                {
                    Covid19TestKit.EmptySequenceInstException _ => Problem(
                        type: "psibr:liminality:samples:covid19-test-kit:empty-sequence",
                        title: "The sequence cannot be empty.",
                        instance: $"psibr:liminality:examples:covid19-test-kit:empty-sequence:{sample.Id ?? ""}",
                        statusCode: 400),
                    _ => throw new NotSupportedException("Unexpected rejection or failure", rejectionOrFailure.Exceptions)
                },
                _ => StatusCode(500) // unhandled
            };
        }
    }
}
