namespace Soap.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    public interface IBlobStorage
    {
        Task<Blob> GetBlob(Guid id, string containerName = "content");

        Task SaveApiMessageAsBlob<T>(T message) where T : ApiMessage;
        Task SaveBase64StringAsBlob(string base64, Guid id, string mimeType);
        Task SaveObjectAsBlob<T>(T @object, Func<T, Guid> getIdFromObject);
    }
}