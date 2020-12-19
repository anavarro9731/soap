namespace Soap.Api.Sample.Afs
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Api;
    using Soap.Utility.Functions.Extensions;

    public static class ValidateMessage
    {
        [FunctionName("ValidateMessage")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);

                string type = req.Query["type"];
                var t = Type.GetType(type);
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var msg = (ApiMessage)requestBody.FromJson(t, SerialiserIds.ApiBusMessage);
                msg.Validate();
                return new OkResult();
            }
            catch (Exception e)
            {
                return new OkObjectResult(e.Message);
            }
        }
    }
}
