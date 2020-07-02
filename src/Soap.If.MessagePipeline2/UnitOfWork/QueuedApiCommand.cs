namespace Soap.MessagePipeline.UnitOfWork
{
    using Soap.Interfaces;

    public class QueuedApiCommand : QueuedStateChange
    {
        public ApiCommand Command { get; set; } 
        //.. here for reference in debugging as all detail is in the closure of the base class
    }
}