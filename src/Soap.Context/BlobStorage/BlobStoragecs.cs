namespace Soap.Context.BlobStorage
{
    using System;
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
            var client = new BlobServiceClient(this.blobStorageSettings.ConnectionString);
            var properties = await client.GetPropertiesAsync();
            properties.Value.Cors = new List<BlobCorsRule>()
            {
                new BlobCorsRule()
                {
                    AllowedMethods = "GET,PUT",
                    AllowedOrigins = "*",
                    MaxAgeInSeconds = 1000,
                    AllowedHeaders = "*",
                    ExposedHeaders = "*"
                }
            };
            
            await client.SetPropertiesAsync(properties);
            
            // Create a SAS token that's valid for one hour.
            AccountSasBuilder sasBuilder = new AccountSasBuilder()
            {
                Services = AccountSasServices.All,
                ResourceTypes = AccountSasResourceTypes.All,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
                Protocol = SasProtocol.HttpsAndHttp
            };
            
            sasBuilder.SetPermissions(AccountSasPermissions.Read |
                                      AccountSasPermissions.Write);
            
            // Use the key to get the SAS token.
            string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential("devstoreaccount1","Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==")).ToString();
            Console.WriteLine("Azurite SasToken:" + sasToken);

        }

        public async Task<ApiMessage> GetApiMessageFromBlob(Guid blobId)
        {
            var blob = await GetBlob(blobId);
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
                var client = @event.StorageSettings.CreateClient(@event.BlobId.ToString(), @event.ContainerName);
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
                var client = @event.StorageSettings.CreateClient(@event.BlobId.ToString(), @event.ContainerName);
                var result = await client.ExistsAsync();
                return result;
            }
        }

        public string GetStorageSasTokenForBlob(Guid blobId, EnumerationFlags permissions)
        {
            return this.blobStorageSettings.MessageAggregator
                       .CollectAndForward(new Events.BlobGetSasTokenEvent(blobId.ToString()))
                       .To(GetToken);

            string GetToken(Events.BlobGetSasTokenEvent args)
            {
                var blobClient = this.blobStorageSettings.CreateClient(args.BlobId);

                //* Check whether this BlobClient object has been authorized with Shared Key.
                Guard.Against(
                    !blobClient.CanGenerateSasUri,
                    "BlobClient must be authorized with Shared Key credentials to create a service SAS.");

                // Create a SAS token that's valid for one hour.
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                };

                sasBuilder.SetPermissions(
                    permissions switch
                    {
                        _ when permissions.HasFlag(IBlobStorage.BlobSasPermissions.ReadAndDelete) => BlobSasPermissions.Read
                            | BlobSasPermissions.Delete,
                        _ when permissions.HasFlag(IBlobStorage.BlobSasPermissions.CreateNew) => BlobSasPermissions.Create,
                        _ => throw new ApplicationException("Must specify an accepted set of blob permissions")
                    });

                var sasUri = blobClient.GenerateSasUri(sasBuilder);
                var sasToken = sasUri.Query;
                return sasToken;
            }
        }

        public async Task SaveApiMessageAsBlob<T>(T message) where T : ApiMessage
        {
            await this.blobStorageSettings.MessageAggregator
                      .CollectAndForward(new Events.BlobUploadEvent(this.blobStorageSettings, message.ToBlob()))
                      .To(Upload);
        }

        public async Task SaveBase64StringAsBlob(string base64, Guid id, string mimeType)
        {
            await this.blobStorageSettings.MessageAggregator
                      .CollectAndForward(new Events.BlobUploadEvent(this.blobStorageSettings, base64.ToBlob(id, mimeType)))
                      .To(Upload);
        }

        public async Task SaveByteArrayAsBlob(byte[] bytes, Guid id, string mimeType)
        {
            await this.blobStorageSettings.MessageAggregator.CollectAndForward(
                          new Events.BlobUploadEvent(this.blobStorageSettings, bytes.ToBlob(id, mimeType)))
                      .To(Upload);
        }

        public async Task SaveObjectAsBlob<T>(T @object, Func<T, Guid> getId, SerialiserIds serialiserId)
        {
            var objectId = getId(@object);

            await this.blobStorageSettings.MessageAggregator.CollectAndForward(
                          new Events.BlobUploadEvent(this.blobStorageSettings, @object.ToBlob(objectId, serialiserId)))
                      .To(Upload);
        }

        private static async Task Upload(Events.BlobUploadEvent args)
        {
            var client = args.StorageSettings.CreateClient(args.Blob.Id.ToString());

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
                public BlobGetSasTokenEvent(string blobId)
                {
                    BlobId = blobId;
                }

                public string BlobId { get; set; }
            }

            public class BlobUploadEvent : IMessage
            {
                public BlobUploadEvent(Settings storageSettings, Blob blob)
                {
                    StorageSettings = storageSettings;
                    Blob = blob;
                }

                public Blob Blob { get; }

                public Settings StorageSettings { get; }
            }
        }

        public class Settings
        {
            public Settings(string connectionString, IMessageAggregator messageAggregator)
            {
                ConnectionString = connectionString;
                MessageAggregator = messageAggregator;
            }

            public IMessageAggregator MessageAggregator { get; }

            public string ConnectionString { get; }

            public BlobClient CreateClient(string blobName, string containerName = "content", BlobClientOptions options = null)
            {
                var container = new BlobContainerClient(ConnectionString, containerName);
                container.CreateIfNotExists(PublicAccessType.Blob);
                var client = new BlobClient(ConnectionString, containerName, blobName, options);
                return client;
            }
        }
    }
}
