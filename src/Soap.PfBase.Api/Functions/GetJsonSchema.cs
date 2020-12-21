namespace Soap.PfBase.Api.Functions
{
    using System;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    public static class Functions
    {
        public static IActionResult GetJsonSchema(ILogger log, Assembly messagesAssembly)
        {
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);

                AzureFunctionContext.LoadAppConfig(out var appConfig);

                dynamic result = DiagnosticFunctions.GetSchema(appConfig, messagesAssembly).AsJson;

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                logger?.Fatal(e, $"Could not execute function {nameof(GetJsonSchema)}");
                log.LogCritical(e.ToString());
                return new OkObjectResult(e.ToString());
            }
        }
    }
}
