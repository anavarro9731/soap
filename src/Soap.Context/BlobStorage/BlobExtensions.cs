namespace Soap.Context.BlobStorage
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using DataStore.Models.PureFunctions.Extensions;
    using Newtonsoft.Json;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public static class BlobExtensions
    {
        public static Blob ToBlob(this string base64String, Guid id, string mimeType)
        {
            Guard.Against(
                !Regex.IsMatch(base64String, "^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$"),
                "Data is invalid format",
                "Blob is not base64 formatted");

            var bytes = Convert.FromBase64String(base64String);

            var blob = new Blob(id, bytes, new Blob.BlobType(mimeType, Blob.TypeClass.Mime));

            return blob;
        }

        public static Blob ToBlob(this object o, Guid id, SerialiserIds serialiserId)
        {
            var blob = new Blob(
                id,
                Encoding.UTF8.GetBytes(o.ToJson(serialiserId)),
                new Blob.BlobType(o.GetType().ToShortAssemblyTypeName(), Blob.TypeClass.AssemblyQualifiedName));
            return blob;
        }

        public static Blob ToBlob(this ApiMessage o) => o.ToBlob(o.Headers.GetMessageId(), SerialiserIds.ApiBusMessage);
        
        public static ApiMessage ToMessage(this Blob b)
        {
            var json = Encoding.UTF8.GetString(b.Bytes);
            var message = json.FromJson<ApiMessage>(SerialiserIds.ApiBusMessage, b.Type.TypeString);
            return message;
        }
        
        
    }
}