namespace Soap.PfBase.Tests
{
    using System;
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces;
    using FluentAssertions;
    using Soap.Context;
    using Soap.Context.BlobStorage;
    using Soap.Context.Exceptions;
    using Soap.Context.Logging;
    using Soap.Context.UnitOfWork;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.NotificationServer;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Soap.Utility;

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
        public MessageLogEntry GetMessageLogEntry()
        {
            //* will be null in beforerunhooks
            return DataStore.ReadById<MessageLogEntry>(FromMessage.Headers.GetMessageId(), options => options.ProvidePartitionKeyValues(WeekInterval.FromUtcNow())).Result;
        }

        public UnitOfWork GetUnitOfWork()
        {
            //* will be null in beforerunhooks
            return  BlobStorage.GetBlobOrError(this.FromMessage.Headers.GetMessageId(), "units-of-work").Result.ToUnitOfWork();    
        }

        
        public ApiMessage FromMessage;
        
        public IMessageAggregator MessageAggregator;
        
        public ProcessState ActiveProcessState;

        public DataStore DataStore;

        public IBus MessageBus;

        public IBlobStorage BlobStorage;

        public NotificationServer NotificationServer;

        public bool Success;

        public Exception UnhandledError { get; set; }
    }
    
    
}
