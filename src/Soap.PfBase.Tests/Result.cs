namespace Soap.PfBase.Tests
{
    using System;
    using DataStore;
    using FluentAssertions;
    using Soap.Context;
    using Soap.Context.Exceptions;
    using Soap.Interfaces;
    using Soap.NotificationServer;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public static class ResultMethods
    {
        public static void ExceptionContainsErrorCode(this Result r, ErrorCode e)
        {
            (r.UnhandledError as FormattedExceptionInfo.PipelineException).KnownErrorCodes.Should().Contain(Guid.Parse(e.Key));
        }
    }
    
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
