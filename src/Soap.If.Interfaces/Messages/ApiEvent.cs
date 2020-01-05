namespace Soap.If.Interfaces.Messages
{
    using System;
    using System.Threading.Tasks;

    /*
    NEVER add any logic to these classes, or you may risk conflicts between versions of message 
    contract assemblies. Use headers to implement variables logic. If you are going to use 
    static classes as a base interface for messages then you must make sure you never add any logic
    which is not backwards compatible. 

        The handle and validate methods being abstract on the class are really powerful.
        They are an implicit, ideally the only implicit way to match an incoming message to
        an associated set of logic without having a reflective process over a set of registered handlers
        i.e. a router which is significantly more complex. You can use dynamic against a set of handle()
        methods but it has its shortcoming which I cannot recall and is not anywhere near as elegant.
     */
    public abstract class ApiEvent : ApiMessage
    {
        public DateTime OccurredAt { get; set; }

        //- see class note
        public abstract Task Handle();
    }
}