using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AspNetCoreExample.Controllers
{
    [ApiController]
    [Route("assays/[controller]")]
    public class Covid19Controller : ControllerBase
    {
        private readonly ILogger<Covid19Controller> _logger;

        public Covid19Controller(
            SARSCoV2Assay covid19Assay,
            ILogger<Covid19Controller> logger)
        {
            Covid19Assay = covid19Assay;
            _logger = logger;
        }

        public SARSCoV2Assay Covid19Assay { get; }

        [HttpPost]
        public async ValueTask<bool?> PostAsync(SARSCoV2Assay.BiologicalSequenceSample sample, CancellationToken cancellationToken = default)
        {
            var (success, result) = await Covid19Assay.SignalAsync(sample, cancellationToken).ConfigureAwait(false);

            if(!success) throw new Exception();

            return result switch
            {
                SARSCoV2Assay.Positive _ => true,
                SARSCoV2Assay.Negative _ => false,
                SARSCoV2Assay.Inconclusive _ => null,
                _ => throw new Exception()
            };
        }
    }
}
