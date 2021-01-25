namespace Soap.Interfaces.Messages
{
    /*
    NEVER add any logic to these classes, or you may risk conflicts between versions of message 
    contract assemblies. Use headers to implement variables logic. If you are going to use 
    static classes as a base interface for messages then you must make sure you never add any logic
    which is not backwards compatible. 
    
    Events are 1-many, one published to many recipients.
    They have no security context even if they result in changes to data, no only because its
    impractical since there are many recipients planning different actions, but because it
    represents something that has already been done, not something that need to be done.
     */
    public abstract class ApiEvent : ApiMessage
    {
    }
}
