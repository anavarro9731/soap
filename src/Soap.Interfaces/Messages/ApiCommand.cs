namespace Soap.Interfaces.Messages
{
    using System;

    public abstract class ApiCommand : ApiMessage, IApiCommand
    {
        public Guid? StatefulProcessId { get; set; }
    }
}