namespace Soap.Interfaces.Messages
{
    using System;

    public abstract class ApiEvent : IApiEvent
    {
        public string IdentityToken { get; set; }

        public Guid MessageId { get; set; }

        public DateTime OccurredAt { get; set; }

        public DateTime? TimeOfCreationAtOrigin { get; set; }
    }
}