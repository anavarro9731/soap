namespace Soap.PfBase.Tests
{
    using System;
    using DataStore;
    using Soap.Interfaces;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.NotificationServer;

    public class Result
    {
        public ProcessState ActiveProcessState;

        public DataStore DataStore;

        public IBus MessageBus;

        public NotificationServer NotificationServer;

        public bool Success;

        public Exception UnhandledError { get; set; }
    }
}