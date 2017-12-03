namespace Soap.If.Interfaces.Messages
{
    using System;

    public interface IApiCommand : IApiMessage
    {
        Guid? StatefulProcessId { get; set; }
    }
}