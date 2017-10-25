namespace Soap.Interfaces.Messages
{
    using System;

    public interface IApiCommand : IApiMessage
    {
        Guid? StatefulProcessId { get; set; }
    }
}