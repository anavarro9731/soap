namespace Soap.Api.Sample.Afs
{
    using System;
    using System.Threading.Tasks;
    using global::Sample.Logic;
    using global::Sample.Messages.Commands;
    using global::Sample.Models.Aggregates;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Soap.Pf.HttpEndpointBase.Controllers;
    using Soap.PfBase.Api;

    public static class CheckHealth
    {
        [FunctionName("CheckHealth")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            HelperFunctions.SetConfigIdForLocalDevelopment();

            try
            {
                AzureFunctionContext.LoadAppConfig(out var logger, out var appConfig);

                dynamic result = new
                {
                    Config = DiagnosticFunctions.GetConfig(appConfig),
                    MessageTest = await DiagnosticFunctions.ExecuteMessageTest<C100Ping, User>(
                                      new C100Ping(),
                                      appConfig,
                                      new MappingRegistration(),
                                      logger)
                };

                string jsonToReturn = JsonConvert.SerializeObject(result, Formatting.Indented);
                jsonToReturn = jsonToReturn.Replace(Environment.NewLine, "<br/>");
                jsonToReturn = jsonToReturn.Replace(" ", "&nbsp");

                return new ContentResult
                {
                    Content = jsonToReturn,
                    ContentType = "text/html"
                };
            }
            catch (Exception e)
            {
                log.LogCritical(e.ToString());
                return new OkObjectResult(e.ToString());
            }
        }
    }
}