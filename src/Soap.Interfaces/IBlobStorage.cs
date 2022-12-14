namespace Soap.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public interface IBlobStorage
    {
        Task DeleteIfExists(Guid id, string containerName = "content");

        Task<bool> Exists(Guid id, string containerName = "content");

        Task<ApiMessage> GetApiMessageFromBlob(Guid blobId);

        Task<Blob> GetBlobOrError(Guid id, string containerName = "content");

        Task<Blob> GetBlobOrNull(Guid id, string containerName = "content");

        string GetStorageSasTokenForBlob(Guid blobId, EnumerationFlags permissions, string containerName = "content");

        Task SaveApiMessageAsBlob<T>(T message) where T : ApiMessage;

        Task SaveBase64StringAsBlob(string base64, Guid id, string mimeType, string containerName = "content");

        Task SaveByteArrayAsBlob(byte[] bytes, Guid id, string mimeType, string containerName = "content");

        Task SaveObjectAsBlob<T>(T @object, Func<T, Guid> getId, SerialiserIds serialiserId, string containerName = "content");

        public class BlobSasPermissions : TypedEnumeration<BlobSasPermissions>
        {
            public static BlobSasPermissions CreateNew = Create("create", nameof(CreateNew));

            public static BlobSasPermissions ReadAndDelete = Create("read-and-delete", nameof(ReadAndDelete));
        }
    }
}