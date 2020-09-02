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

            var asText = req.Query["format"] == "html";
            
            HelperFunctions.SetAppKey();

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

                if (asText)
                {
                    string jsonAsHtml = JsonConvert.SerializeObject(result, Formatting.Indented);
                    jsonAsHtml = jsonAsHtml.Replace(Environment.NewLine, "<br/>");
                    jsonAsHtml = jsonAsHtml.Replace(" ", "&nbsp");

                    string html = @$"<!DOCTYPE html>
                        <html lang=""en"">
                        <head>
                        <meta charset=""utf-8"">
                        <title>Check Health</title>
                        </head>
                        <body style=""font-family:Lucida Console"">
                        {jsonAsHtml}
                        </body>
                        </html>";
                    
                    return new ContentResult
                    {
                        Content = html,
                        ContentType = "text/html"
                    };
                }
                else
                {
                    return new JsonResult(result);
                }
            }
            catch (Exception e)
            {
                log.LogCritical(e.ToString());
                return new OkObjectResult(e.ToString());
            }
        }
    }
}