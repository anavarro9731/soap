namespace Soap.If.MessagePipeline.Messages
{
    using Soap.If.Interfaces.Messages;

    public class QueuedApiCommand : QueuedStateChange
    {
        public IApiCommand Command { get; set; } 
        //here for reference in debugging as all detail is in the closure of the base class
    }
}