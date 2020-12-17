namespace Soap.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public interface IBlobStorage
    {
        Task<ApiMessage> GetApiMessageFromBlob(Guid blobId);

        Task<Blob> GetBlob(Guid id, string containerName = "content");
        
        Task<bool> Exists(Guid id, string containerName = "content");

        string GetStorageSasTokenForBlob(Guid blobId, EnumerationFlags permissions);

        Task SaveApiMessageAsBlob<T>(T message) where T : ApiMessage;

        Task SaveBase64StringAsBlob(string base64, Guid id, string mimeType);

        Task SaveByteArrayAsBlob(byte[] bytes, Guid id, string mimeType);

        Task SaveObjectAsBlob<T>(T @object, Func<T, Guid> getId, SerialiserIds serialiserId);

        public class BlobSasPermissions : TypedEnumeration<BlobSasPermissions>
        {
            public static BlobSasPermissions CreateNew = Create("create", nameof(CreateNew));

            public static BlobSasPermissions ReadAndDelete = Create("read-and-delete", nameof(ReadAndDelete));
        }
    }
}
