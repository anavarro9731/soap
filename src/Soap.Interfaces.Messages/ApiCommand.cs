namespace Soap.Interfaces.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /* 
    NEVER add any logic to these classes, or you may risk conflicts between versions of message 
    contract assemblies. Use headers to implement variables logic. If you are going to use 
    static classes as a base interface for messages then you must make sure you never add any logic
    which is not backwards compatible. 
    
    Commands are many-1. Many sources to one recipient.
    They have a security context as they represent something that needs to be done,
    and not everyone may have that permission.
     */

    public abstract class ApiCommand : ApiMessage
    {
    }

    
}
