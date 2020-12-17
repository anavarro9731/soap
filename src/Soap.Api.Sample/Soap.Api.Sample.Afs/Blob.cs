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
    using Soap.Context;
    using Soap.Context.BlobStorage;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.PfBase.Api;
    using Soap.Utility.Enums;
    using Soap.Utility.Functions.Extensions;

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

                GetBlobId(req, out var id);

                var blobClient = new BlobStorage(
                    new BlobStorage.Settings(appConfig.StorageConnectionString, new MessageAggregator()));

                var blob = await blobClient.GetBlob(id);

                return new FileContentResult(blob.Bytes, blob.Type.TypeString);


            }
            catch (Exception e)
            {
                logger?.Fatal(e, "Could not execute function");
                log.LogCritical(e.ToString());
                return new OkObjectResult(e.ToString());
            }
        }
        static void GetBlobId(HttpRequest req, out Guid blobId)
        {
            string idParameter = req.Query["id"];
            var parsed = Guid.TryParse(idParameter, out var id);
            Guard.Against(
                !parsed,
                "Could not parse ID of Image from Querystring param 'id'",
                ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly);
            blobId = id;
        }
        
        [FunctionName("AddBlob")]
        public static async Task<IActionResult> AddBlob(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);

                AzureFunctionContext.LoadAppConfig(out var appConfig);

                var blobClient = new BlobStorage(
                    new BlobStorage.Settings(appConfig.StorageConnectionString, new MessageAggregator()));

                var bytes = await GetBytes();

                GetBlobId(req, out var id);

                await blobClient.SaveByteArrayAsBlob(bytes, id, req.ContentType);

                return new OkResult();
            }
            catch (Exception e)
            {
                logger?.Fatal(e, "Could not execute function");
                log.LogCritical(e.ToString());
                return new OkObjectResult(e.ToString());
            }

            async Task<byte[]> GetBytes()
            {
                await using var memoryStream = new MemoryStream();
                await req.Body.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();
                return bytes;
            }
        }
    }
}
