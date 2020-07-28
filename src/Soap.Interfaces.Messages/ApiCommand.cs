namespace Soap.Interfaces.Messages
{
    /* 
    NEVER add any logic to these classes, or you may risk conflicts between versions of message 
    contract assemblies. Use headers to implement variables logic. If you are going to use 
    static classes as a base interface for messages then you must make sure you never add any logic
    which is not backwards compatible. 
     */

    public abstract class ApiCommand : ApiMessage
    {
    }
}