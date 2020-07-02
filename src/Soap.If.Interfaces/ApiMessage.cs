namespace Soap.Interfaces
{
    using System;
    using CircuitBoard.Messages;
    using Soap.Pf.MessageContractsBase.Commands;

    /*
    NEVER add any logic to these classes, or you may risk conflicts between versions of message 
    contract assemblies. Use headers to implement variable logic. If you are going to use 
    static classes as a base interface for messages then you must make sure you never add any logic
    which is not backwards compatible. 

    Interfaces are nice and can now have default impl but they can't access the impl inside the class
    one big downside, and they clutter up the class with all the repetitive properties that don't need
    a default impl so in most cases abstract classes are still preferable where a Is-A relation exists
     */

    public abstract class ApiMessage : IMessage
    {
        public string IdentityToken { get; set; }

        public Guid MessageId { get; set; }

        public DateTime? TimeOfCreationAtOrigin { get; set; }

        public ApiPermission ApiPermission { get; set; }

    }
}