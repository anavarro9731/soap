namespace Soap.Bus
{
    using System;
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    public class QueuedEventToPublish : IQueuedBusOperation
    {

        public bool Committed { get; set; }

        public ApiEvent EventToPublish { get; set; }
    }
}