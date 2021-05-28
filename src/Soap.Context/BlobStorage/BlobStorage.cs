namespace Soap.Context.BlobStorage
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Storage;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Azure.Storage.Blobs.Specialized;
    using Azure.Storage.Sas;
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;

    /* doesn't need it's own project right now because it is not needed by the Soap.Config repo
     since at present config only stores connection string and not a settings object and that 
     is because there are no settings right now. that could all change of course */

    public class BlobStorage : IBlobStorage
    {
        private readonly Settings blobStorageSettings;

        public BlobStorage(Settings blobStorageSettings)
        {
            this.blobStorageSettings = blobStorageSettings;
        }

        public async Task DevStorageSetup()
        {
            var blobServiceClient = this.blobStorageSettings.GetServiceClient;
            var properties = await blobServiceClient.GetPropertiesAsync();
            int sasTokenExpiryInSecondsFromNow = 1000;
            if (properties.Value.Cors.Count == 0)
            {
                properties.Value.Cors = new List<BlobCorsRule>()
                {
                    new BlobCorsRule()
                    {
                        AllowedMethods = "GET,PUT,POST,DELETE,OPTIONS",
                        AllowedOrigins = "*",
                        MaxAgeInSeconds = sasTokenExpiryInSecondsFromNow,
                        AllowedHeaders = "*",
                        ExposedHeaders = "*"
                    }
                };
                await blobServiceClient.SetPropertiesAsync(properties);
            }
            
            // Create a SAS token that's valid for one hour.
            AccountSasBuilder sasBuilder = new AccountSasBuilder()
            {
                Services = AccountSasServices.All,
                ResourceTypes = AccountSasResourceTypes.All,
                ExpiresOn = GetSasExpiry,
                Protocol = SasProtocol.HttpsAndHttp
            };
            
            sasBuilder.SetPermissions(AccountSasPermissions.Read |
                                      AccountSasPermissions.Write);
            
            // Use the key to get the SAS token.
            string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential("devstoreaccount1","Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==")).ToString();
            
            Console.WriteLine($"Azurite Test SasToken Valid For The Timespan [days:hours:minutes:seconds] {new TimeSpan(0,0,0,sasTokenExpiryInSecondsFromNow):g} : {sasToken}");

        }

        public async Task<ApiMessage> GetApiMessageFromBlob(Guid blobId)
        {
            var blob = await GetBlob(blobId, "large-messages");
            return blob.ToMessage();
        }

        public async Task<Blob> GetBlob(Guid id, string containerName = "content")
        {
            try
            {
                var blob = await this.blobStorageSettings.MessageAggregator
                                     .CollectAndForward(new Events.BlobDownloadEvent(this.blobStorageSettings, id, containerName))
                                     .To(Download);
                return blob;
            }
            catch (RequestFailedException r)
            {
                throw new ApplicationException($"Could not read blob with id {id} from storage", r);
            }

            static async Task<Blob> Download(Events.BlobDownloadEvent @event)
            {
                var client = @event.StorageSettings.CreateBlobClient(@event.BlobId.ToString(), @event.ContainerName);
                var result = await client.DownloadAsync();
                await using var memoryStream = new MemoryStream();
                await result.Value.Content.CopyToAsync(memoryStream);
                var allBytes = memoryStream.ToArray();
                var typeString = result.Value.Details.Metadata["typeString"];
                var typeClass = result.Value.Details.Metadata["typeClass"];
                return new Blob(
                    @event.BlobId,
                    allBytes,
                    new Blob.BlobType(typeString, TypedEnumeration<Blob.TypeClass>.GetInstanceFromKey(typeClass)));
            }
        }

        public async Task<bool> Exists(Guid id, string containerName = "content")
        {
            try
            {
                var exists = await this.blobStorageSettings.MessageAggregator
                                     .CollectAndForward(new Events.BlobDownloadEvent(this.blobStorageSettings, id, containerName))
                                     .To(Exists);
                return exists;
            }
            catch (RequestFailedException r)
            {
                throw new ApplicationException($"Could not read blob with id {id} from storage", r);
            }

            static async Task<bool> Exists(Events.BlobDownloadEvent @event)
            {
                var client = @event.StorageSettings.CreateBlobClient(@event.BlobId.ToString(), @event.ContainerName);
                var result = await client.ExistsAsync();
                return result;
            }
        }

        public string GetStorageSasTokenForBlob(Guid blobId, EnumerationFlags permissions, string containerName = "content")
        {
            return this.blobStorageSettings.MessageAggregator
                       .CollectAndForward(new Events.BlobGetSasTokenEvent(blobId.ToString(), containerName))
                       .To(GetToken);

            string GetToken(Events.BlobGetSasTokenEvent args)
            {
                // Create a SAS token that's valid for one hour.
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = args.ContainerName,
                    BlobName = args.BlobId,
                    Resource = "b",
                    ExpiresOn = GetSasExpiry
                };

                sasBuilder.SetPermissions(
                    permissions switch
                    {
                        _ when permissions.HasFlag(IBlobStorage.BlobSasPermissions.ReadAndDelete) => BlobSasPermissions.Read
                            | BlobSasPermissions.Delete,
                        _ when permissions.HasFlag(IBlobStorage.BlobSasPermissions.CreateNew) => BlobSasPermissions.Create,
                        _ => throw new ApplicationException("Must specify an accepted set of blob permissions")
                    });
                
                var sasToken = sasBuilder.ToSasQueryParameters(this.blobStorageSettings.GeStorageKeyCredential());

                return "?" + sasToken.ToString();
            }
        }

        private DateTimeOffset GetSasExpiry => DateTimeOffset.UtcNow.AddHours(1); 

        public async Task SaveApiMessageAsBlob<T>(T message) where T : ApiMessage
        {
            await this.blobStorageSettings.MessageAggregator
                      .CollectAndForward(new Events.BlobUploadEvent(this.blobStorageSettings, message.ToBlob(), "large-messages"))
                      .To(Upload);
        }

        public async Task SaveBase64StringAsBlob(string base64, Guid id, string mimeType, string containerName = "content")
        {
            await this.blobStorageSettings.MessageAggregator
                      .CollectAndForward(new Events.BlobUploadEvent(this.blobStorageSettings, base64.ToBlob(id, mimeType), containerName))
                      .To(Upload);
        }

        public async Task SaveByteArrayAsBlob(byte[] bytes, Guid id, string mimeType, string containerName = "content")
        {
            await this.blobStorageSettings.MessageAggregator.CollectAndForward(
                          new Events.BlobUploadEvent(this.blobStorageSettings, bytes.ToBlob(id, mimeType), containerName))
                      .To(Upload);
        }

        public async Task SaveObjectAsBlob<T>(T @object, Func<T, Guid> getId, SerialiserIds serialiserId, string containerName = "content")
        {
            var objectId = getId(@object);

            await this.blobStorageSettings.MessageAggregator.CollectAndForward(
                          new Events.BlobUploadEvent(this.blobStorageSettings, @object.ToBlob(objectId, serialiserId), containerName))
                      .To(Upload);
        }

        private static async Task Upload(Events.BlobUploadEvent args)
        {
            var client = args.StorageSettings.CreateBlobClient(args.Blob.Id.ToString(), args.ContainerName);

            await client.UploadAsync(
                new MemoryStream(args.Blob.Bytes),
                metadata: new Dictionary<string, string>
                    { { "typeString", args.Blob.Type.TypeString }, { "typeClass", args.Blob.Type.TypeClass.Key } });
        }

        public static class Events
        {
            public class BlobDownloadEvent : IMessage
            {
                public readonly Guid BlobId;

                public BlobDownloadEvent(Settings storageSettings, Guid blobId, string containerName)
                {
                    StorageSettings = storageSettings;
                    ContainerName = containerName;
                    this.BlobId = blobId;
                }

                public string ContainerName { get; }

                public Settings StorageSettings { get; }
            }

            public class BlobGetSasTokenEvent : IMessage
            {
                public BlobGetSasTokenEvent(string blobId, string containerName)
                {
                    BlobId = blobId;
                    ContainerName = containerName;
                }

                public string BlobId { get; set; }

                public string ContainerName { get; }
            }

            public class BlobUploadEvent : IMessage
            {
                public BlobUploadEvent(Settings storageSettings, Blob blob, string containerName)
                {
                    StorageSettings = storageSettings;
                    Blob = blob;
                    ContainerName = containerName;
                }

                public Blob Blob { get; }

                public string ContainerName { get; }

                public Settings StorageSettings { get; }
            }
        }

        public class Settings
        {
            private static BlobServiceClient blobServiceClient;

            private static ConcurrentDictionary<string, BlobContainerClient> BlobContainerClients =
                new ConcurrentDictionary<string, BlobContainerClient>();

            private static string ConnectionString;

            public Settings(string connectionString, IMessageAggregator messageAggregator)
            {
                MessageAggregator = messageAggregator;
                blobServiceClient ??= new BlobServiceClient(connectionString);
                ConnectionString ??= connectionString;
            }

            public StorageSharedKeyCredential GeStorageKeyCredential()
            {
                var parsedConnectionString = new Dictionary<string, string>();
                foreach (var item in ConnectionString.Split(';'))
                {
                    if (string.IsNullOrWhiteSpace(item)) continue;
                    var idx = item.IndexOf('=');
                    parsedConnectionString[item.Substring(0, idx)] = item.Substring(idx + 1, item.Length - idx - 1);
                }

                return new StorageSharedKeyCredential(parsedConnectionString["AccountName"], parsedConnectionString["AccountKey"]);
            }

            public IMessageAggregator MessageAggregator { get; }

            internal BlobServiceClient GetServiceClient => blobServiceClient;
            
            public BlobClient CreateBlobClient(string blobName, string containerName = "content")
            {
                var containerClient = BlobContainerClients.GetOrAdd(containerName, (key) => blobServiceClient.GetBlobContainerClient(key));
                containerClient.CreateIfNotExists(PublicAccessType.Blob);

                var blobClient = containerClient.GetBlobClient(blobName);
                return blobClient;
            }
        }
    }
}
