namespace Soap.Context.BlobStorage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Soap.Utility.Functions.Operations;

    public class BlobStorage
    {
        private readonly Settings blobStorageSettings;

        public BlobStorage(Settings blobStorageSettings)
        {
            this.blobStorageSettings = blobStorageSettings;
        }

        public async Task<Blob> GetBlob(Guid id)
        {
            var client = this.blobStorageSettings.CreateClient(id.ToString());
            try
            {
                var result = await client.DownloadAsync();
                await using var memoryStream = new MemoryStream();
                await result.Value.Content.CopyToAsync(memoryStream);
                var allBytes = memoryStream.ToArray();
                var mimeType = result.Value.Details.Metadata["mimeType"];
                return new Blob
                {
                    Bytes = allBytes,
                    MimeType = mimeType
                };
            }
            catch (RequestFailedException r)
            {
                throw new Exception($"Could not read blob with id {id} from storage", r);
            }
        }

        public async Task SaveBlobFromBase64String(Guid id, string base64blob, string mimeType)
        {
            Guard.Against(
                !Regex.IsMatch(base64blob, "^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$"),
                "Data is invalid format",
                "Blob is not base64 formatted");

            var client = this.blobStorageSettings.CreateClient(id.ToString());

            var bytes = Convert.FromBase64String(base64blob);
            var stream = new MemoryStream(bytes);

            await client.UploadAsync(stream, metadata: new Dictionary<string, string> { { "mimeType", mimeType } });
        }

        public class Blob
        {
            public byte[] Bytes { get; set; }

            public string MimeType { get; set; }
        }

        public class Settings
        {
            public Settings(string connectionString)
            {
                ConnectionString = connectionString;
            }

            private string ConnectionString { get; }

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