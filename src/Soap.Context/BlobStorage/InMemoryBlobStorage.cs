namespace Soap.Context.BlobStorage
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Azure;
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    /* doesn't need it's own project right now because it is not needed by the Soap.Config repo
     since at present config only stores connection string and not a settings object and that 
     is because there are no settings right now. that could all change of course */

    public class InMemoryBlobStorage : IBlobStorage
    {
        private readonly Dictionary<string, Blob> blobs = new Dictionary<string, Blob>();

        private readonly Settings blobStorageSettings;

        public InMemoryBlobStorage(Settings blobStorageSettings)
        {
            this.blobStorageSettings = blobStorageSettings;
        }

        private DateTimeOffset GetSasExpiry => DateTimeOffset.UtcNow.AddHours(1);

        public Task<Blob> GetBlobOrNull(Guid id, string containerName = "content")
        {
            try
            {
                return Task.FromResult(Download(new Events.BlobDownloadEvent(this.blobStorageSettings, id, containerName)));
            }
            catch (KeyNotFoundException)
            {
                return Task.FromResult<Blob>(null);
            }

            Blob Download(Events.BlobDownloadEvent @event)
            {
                return this.blobs[@event.BlobId + @event.ContainerName];
            }
        }

        public Task<bool> Exists(Guid id, string containerName = "content")
        {
            try
            {
                var exists = this.blobs.ContainsKey(id + containerName);
                return Task.FromResult(exists);
            }
            catch (RequestFailedException r)
            {
                throw new CircuitException($"Could not read blob with id {id} from storage", r);
            }
        }

        public async Task<ApiMessage> GetApiMessageFromBlob(Guid blobId)
        {
            var blob = await GetBlobOrError(blobId, "large-messages");
            return blob.ToMessage();
        }

        public Task<Blob> GetBlobOrError(Guid id, string containerName = "content")
        {
            try
            {
                return Task.FromResult(Download(new Events.BlobDownloadEvent(this.blobStorageSettings, id, containerName)));
            }
            catch (RequestFailedException r)
            {
                throw new CircuitException($"Could not read blob with id {id} from storage", r);
            }

            Blob Download(Events.BlobDownloadEvent @event)
            {
                return this.blobs[@event.BlobId + @event.ContainerName];
            }
        }

        public string GetStorageSasTokenForBlob(Guid blobId, EnumerationFlags permissions, string containerName = "content")
        {
            return GetToken(new Events.BlobGetSasTokenEvent(blobId.ToString(), containerName));

            string GetToken(Events.BlobGetSasTokenEvent args)
            {
                // Create a SAS token that's valid for one hour.
                return "fake-token";
            }
        }

        public Task SaveApiMessageAsBlob<T>(T message) where T : ApiMessage
        {
            Upload(new Events.BlobUploadEvent(this.blobStorageSettings, message.ToBlob(), "large-messages"));

            return Task.CompletedTask;
        }

        public Task SaveBase64StringAsBlob(string base64, Guid id, string mimeType, string containerName = "content")
        {
            Upload(new Events.BlobUploadEvent(this.blobStorageSettings, base64.ToBlob(id, mimeType), containerName));

            return Task.CompletedTask;
        }

        public Task SaveByteArrayAsBlob(byte[] bytes, Guid id, string mimeType, string containerName = "content")
        {
            Upload(new Events.BlobUploadEvent(this.blobStorageSettings, bytes.ToBlob(id, mimeType), containerName));

            return Task.CompletedTask;
        }

        public Task SaveObjectAsBlob<T>(T @object, Func<T, Guid> getId, SerialiserIds serialiserId, string containerName = "content")
        {
            var objectId = getId(@object);

            Upload(new Events.BlobUploadEvent(this.blobStorageSettings, @object.ToBlob(objectId, serialiserId), containerName));

            return Task.CompletedTask;
        }

        public Task Upload(Events.BlobUploadEvent args)
        {
            
            var key = args.Blob.Id + args.ContainerName;
            if (this.blobs.ContainsKey(key))
            {
                this.blobs[key] = args.Blob;
            }
            else
            {
                this.blobs.Add(key, args.Blob);
            }
            
            
            return Task.CompletedTask;
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
            public Settings()
            {
                
            }
        }
    }
}