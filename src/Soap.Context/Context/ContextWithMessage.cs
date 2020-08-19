﻿namespace Soap.MessagePipeline.Context
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Logging;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Objects.Blended;

    public class ContextWithMessage : BoostrappedContext, IMessageFunctionsServerSide
    {
        private readonly IMessageFunctionsServerSide functions;

        public ContextWithMessage(
            ApiMessage message,
            (DateTime receivedTime, long receivedTicks) timeStamp,
            BoostrappedContext context)
            : base(context)
        {
            Message = message;
            TimeStamp = timeStamp;
            this.functions = this.MessageMapper.MapMessage(message);
        }

        protected ContextWithMessage(ContextWithMessage c)
            : base(c)
        {
            Message = c.Message;
            this.functions = c.functions;
            TimeStamp = c.TimeStamp;
        }

        public ApiMessage Message { get; }

        public (DateTime receivedTime, long receivedTicks) TimeStamp { get; }

        public Dictionary<ErrorCode, ErrorCode> GetErrorCodeMappings() => this.functions.GetErrorCodeMappings();

        public Task Handle(ApiMessage msg) => this.functions.Handle(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) => this.functions.HandleFinalFailure(msg);

        public void Validate(ApiMessage msg) => this.functions.Validate(msg);
    }
    
}