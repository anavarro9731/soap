namespace Soap.If.Interfaces.Messages
{
    using System;

    public class ApiMessage : IApiMessage
    {
        public string IdentityToken { get; set; }

        public Guid MessageId { get; set; }

        public DateTime? TimeOfCreationAtOrigin { get; set; }
    }
}