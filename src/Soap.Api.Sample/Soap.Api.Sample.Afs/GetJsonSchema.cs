namespace Soap.Api.Sample.Afs
{
    using System;
    using System.Reflection.Metadata;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.PfBase.Api;

    public static class GetJsonSchema
    {
        [FunctionName("GetJsonSchema")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);
                
                AzureFunctionContext.LoadAppConfig(out var appConfig);

                dynamic result = DiagnosticFunctions.GetSchema(appConfig, typeof(C100v1_Ping).Assembly).AsJson;

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                logger?.Fatal(e, "Could not execute function");
                log.LogCritical(e.ToString());
                return new OkObjectResult(e.ToString());
            }
        }
    }
}