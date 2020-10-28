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

    public static class GetImage
    {
        [FunctionName("GetImage")]
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

                string id = req.Query["id"];
                
                var blobClient = this.containerClient.GetBlobClient($"{id}");

                var blob = (await blobClient.DownloadAsync());

                return File(blob.Value.Content, blob.Value.ContentType);

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