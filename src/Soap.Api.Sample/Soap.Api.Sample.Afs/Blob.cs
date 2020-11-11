namespace Soap.Api.Sample.Afs
{
    using System;
    using System.Threading.Tasks;
    using Mainwave.MimeTypes;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Soap.Context;
    using Soap.Context.BlobStorage;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.PfBase.Api;
    using Soap.Utility.Enums;
    using Soap.Utility.Functions.Operations;

    public static class GetBlob
    {
        [FunctionName("GetBlob")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);

                AzureFunctionContext.LoadAppConfig(out var appConfig);

                var id = GetImageId();

                var blobClient = new BlobStorage(new BlobStorage.Settings(appConfig.StorageConnectionString, new MessageAggregator()));

                var blob = await blobClient.GetBlob(id);

                return new FileContentResult(blob.Bytes, blob.Type.TypeString);

                Guid GetImageId()
                {
                    string idParameter = req.Query["id"];
                    var parsed = Guid.TryParse(idParameter, out var id);
                    Guard.Against(
                        !parsed,
                        "Could not parse ID of Image from Querystring param 'id'",
                        ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly);
                    return id;
                }
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