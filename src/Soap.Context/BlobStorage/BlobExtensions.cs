namespace Soap.Context.BlobStorage
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility;
    using Soap.Utility.Functions.Operations;

    public static class BlobExtensions
    {
        public static Blob ToBlob(this string s, Guid id, string mimeType)
        {
            Guard.Against(
                !Regex.IsMatch(s, "^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$"),
                "Data is invalid format",
                "Blob is not base64 formatted");

            var bytes = Convert.FromBase64String(s);

            var blob = new Blob(id, bytes, new Blob.BlobType(mimeType, Blob.TypeClass.Mime));

            return blob;
        }

        public static Blob ToBlob(this object o, Guid id)
        {
            var blob = new Blob(
                id,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(o, JsonNetSettings.ApiMessageSerialiserSettings)),
                new Blob.BlobType(o.GetType().AssemblyQualifiedName, Blob.TypeClass.AssemblyQualifiedName));
            return blob;
        }

        public static Blob ToBlob(this ApiMessage o) => o.ToBlob(o.Headers.GetMessageId());
        
        public static ApiMessage ToMessage(this Blob b)
        {
            var type = Type.GetType(b.Type.TypeString);
            var json = Encoding.UTF8.GetString(b.Bytes);
            var message = JsonConvert.DeserializeObject(json, type, JsonNetSettings.ApiMessageDeserialisationSettings) as ApiMessage;
            return message;
        }
        
        
    }
}