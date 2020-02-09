namespace Soap.Interfaces.Messages
{
    using System;
    using System.Threading.Tasks;

    /* these two classes should not inherit from each other so as to allow
     them to be constrained separately to be grouped in lists they both implement IApiCommand

    NEVER add any logic to these classes, or you may risk conflicts between versions of message 
    contract assemblies. Use headers to implement variables logic. If you are going to use 
    static classes as a base interface for messages then you must make sure you never add any logic
    which is not backwards compatible. 
     */

    public abstract class ApiCommandBase : ApiMessage
    {
        public Guid? StatefulProcessId { get; set; }
    }

    public abstract class ApiCommand : ApiCommandBase
    {
        public abstract Task Handle();
    }

    public abstract class ApiCommand<TResponse> : ApiCommandBase, IApiQuery where TResponse : ApiEvent, new()
    {
        public abstract Task<ApiEvent> Handle();
    }
}