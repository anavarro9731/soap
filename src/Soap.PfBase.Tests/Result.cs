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
            switch (r.UnhandledError) 
            {
                case FormattedExceptionInfo.PipelineException e1: 
                    e1.KnownErrorCodes.Should().Contain(Guid.Parse(e.Key));
                    break;
                default:
                 r.UnhandledError.Should().BeOfType<FormattedExceptionInfo.PipelineException>();
                 break;
            }
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
