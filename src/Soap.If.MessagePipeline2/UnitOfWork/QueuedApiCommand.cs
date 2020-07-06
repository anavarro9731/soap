namespace Soap.MessagePipeline.UnitOfWork
{
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class QueuedApiCommand : QueuedStateChange
    {
        public ApiCommand Command { get; set; } 
        //.. here for reference in debugging as all detail is in the closure of the base class
    }
}