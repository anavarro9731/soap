namespace Soap.Api.Sample.Afs
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Pf.HttpEndpointBase.Controllers;
    using Soap.PfBase.Api;

    public static class PrintSchema
    {
        [FunctionName("PrintSchema")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            try
            {
                AzureFunctionContext.LoadAppConfig(out var logger, out var appConfig);

                dynamic result = DiagnosticFunctions.GetSchema(appConfig, typeof(C100Ping).Assembly);

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                log.LogCritical(e.ToString());
                return new OkObjectResult(e.ToString());
            }
        }
    }
}