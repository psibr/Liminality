using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PSIBR.Liminality;

namespace AspNetCoreExample.Controllers
{
    [ApiController]
    [Route("assays/[controller]")]
    public class Covid19Controller : ControllerBase
    {
        private readonly ILogger<Covid19Controller> _logger;

        public Covid19Controller(
            SARSCoV2AssayFactory covid19AssayFactory,
            ILogger<Covid19Controller> logger)
        {
            Covid19AssayFactory = covid19AssayFactory;
            _logger = logger;
        }

        public SARSCoV2AssayFactory Covid19AssayFactory { get; }

        [HttpPost]
        public async ValueTask<IActionResult> PostAsync(SARSCoV2Assay.BiologicalSequenceSample sample, CancellationToken cancellationToken = default)
        {
            var covid19Assay = Covid19AssayFactory.Create(sample.Id);
            var resultValueTask = covid19Assay.SignalAsync(sample, new SignalOptions(throwWhenTransitionNotFound: true), cancellationToken);
            
            if(!resultValueTask.IsCompletedSuccessfully) await resultValueTask;

            ISignalResult result = resultValueTask.Result;

            return result switch
            {
                TransitionedResult transitioned => transitioned.NewState switch
                {
                    SARSCoV2Assay.Positive _ => Ok(1),
                    SARSCoV2Assay.Negative _ => Ok(-1),
                    SARSCoV2Assay.Inconclusive _ => Ok(0),
                    _ => throw new Exception("Assesment ended prematurely")
                },
                RejectedByPreconditionResult rejection => rejection.PreconditionExceptions.InnerException switch
                {
                    SARSCoV2Assay.EmptySequenceInstException _ => Problem(
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
