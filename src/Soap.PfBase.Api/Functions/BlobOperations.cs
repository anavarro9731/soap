namespace Soap.PfBase.Api.Functions
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Soap.Config;
    using Soap.Context;
    using Soap.Context.BlobStorage;
    using Soap.Idaam;
    using Soap.Interfaces;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.Utility;
    using Soap.Utility.Enums;

    public static partial class PlatformFunctions
    {
        public static async Task<IActionResult> AddBlob(HttpRequest req, ILogger log)
        {
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);

                AzureFunctionContext.LoadAppConfig(out var appConfig);

                if (appConfig.AuthLevel.AuthenticationRequired)
                {
                    await VerifyIdToken(req, appConfig);    
                }
                
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

        public static async Task<IActionResult> GetBlob(HttpRequest req, ILogger log)
        {
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);

                AzureFunctionContext.LoadAppConfig(out var appConfig);

                if (appConfig.AuthLevel.AuthenticationRequired)
                {
                    await VerifyIdToken(req, appConfig);    
                }
                
                GetBlobId(req, out var id);

                var blobClient = new BlobStorage(
                    new BlobStorage.Settings(appConfig.StorageConnectionString, new MessageAggregator()));

                var blob = await blobClient.GetBlobOrError(id);

                return new FileContentResult(blob.Bytes, blob.Type.TypeString);
            }
            catch (Exception e)
            {
                logger?.Fatal(e, "Could not execute function");
                log.LogCritical(e.ToString());
                return new OkObjectResult(e.ToString());
            }
        }

        private static void GetBlobId(HttpRequest req, out Guid blobId)
        {
            string idParameter = req.Query["id"];
            var parsed = Guid.TryParse(idParameter, out var id);
            Guard.Against(
                !parsed,
                "Could not parse ID of Image from Querystring param 'id'",
                ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly);
            blobId = id;
        }
        
        private static async Task VerifyIdToken(HttpRequest req, ApplicationConfig appConfig)
        {
            string idToken = req.Query["it"];
            var idaamProvider = new IdaamProvider(appConfig);
            await idaamProvider.GetLimitedUserProfileFromIdentityToken(idToken);
        }
    }
}
