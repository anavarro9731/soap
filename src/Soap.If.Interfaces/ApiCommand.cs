namespace Soap.Interfaces
{
    using System;

    /* these two classes should not inherit from each other so as to allow
     them to be constrained separately to be grouped in lists they both implement ApiCommandBase

    NEVER add any logic to these classes, or you may risk conflicts between versions of message 
    contract assemblies. Use headers to implement variables logic. If you are going to use 
    static classes as a base interface for messages then you must make sure you never add any logic
    which is not backwards compatible. 
     */

    public interface IApiCommand 
    { 
        Guid? StatefulProcessId { get; set; }
    }

    public abstract class ApiCommand : ApiMessage, IApiCommand
    {
        public Guid? StatefulProcessId { get; set; }
    }

    public abstract class ApiCommand<TResponse> : IApiQuery where TResponse : ApiEvent, new()
    {
        public Guid? StatefulProcessId { get; set; }
    }
}