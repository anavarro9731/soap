namespace Soap.Interfaces.Messages
{
    using System;

    public abstract class ApiCommand : IApiCommand
    {
        public string IdentityToken { get; set; }

        public Guid MessageId { get; set; }

        public Guid? StatefulProcessId { get; set; }

        public DateTime? TimeOfCreationAtOrigin { get; set; }
    }
}