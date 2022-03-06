using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PSIBR.Liminality;
using System;

namespace Samples.Controllers
{
    [ApiController]
    [Route("assays/[controller]")]
    public class Covid19Controller : ControllerBase
    {
        [HttpPost]
        public IActionResult Post(Covid19TestKit.BiologicalSequenceSample sample, [FromServices] Covid19TestKit testKit)
        {
            Covid19TestKit covid19Assay = testKit;

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
                RejectedByPreconditionResult rejection => rejection.PreconditionExceptions.InnerException switch
                {
                    Covid19TestKit.EmptySequenceInstException _ => Problem(
                        type: "psibr:liminality:examples:sars-cov-2-assay:sequence-empty",
                        title: "The sequence cannot be empty.",
                        instance: $"psibr:liminality:examples:sars-cov-2-assay:sequence-empty:{sample.Id ?? ""}",
                        statusCode: 400),
                    _ => throw new NotSupportedException("Unexpected precondition rejection", rejection.PreconditionExceptions)
                },
                _ => StatusCode(500) // unhandled
            };
        }
    }
}
