﻿namespace Soap.MessagePipeline.Context
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soap.Interfaces;
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

    internal static class ContextAfterMessageObtainedExtensions
    {
        internal static async Task CreateOrFindLogEntry(
            this ContextWithMessage ctx,
            IApiIdentity identity,
            Action<MessageLogEntry> outLogEntry)
        {
            {
                MessageLogEntry entry = null;

                await FindLogEntry(ctx, v => entry = v);

                if (entry == null)
                {
                    var meta = new MessageMeta(ctx.TimeStamp, identity, ctx.Message.GetSchema());

                    await CreateNewLogEntry(meta, ctx, v => entry = v);
                }

                outLogEntry(entry);
            }

            static async Task CreateNewLogEntry(MessageMeta meta, ContextWithMessage ctx, Action<MessageLogEntry> outLogEntry)
            {
                var message = ctx.Message;

                try
                {
                    ctx.Logger.Debug($"Creating record for msg id {message.MessageId}");

                    var messageLogEntry = new MessageLogEntry(
                        message,
                        meta,
                        ctx.DataStore.DataStoreOptions.OptimisticConcurrency, //TODO
                        ctx.AppConfig.NumberOfApiMessageRetries);

                    var newItem = await ctx.DataStore.Create(messageLogEntry);

                    await ctx.DataStore.CommitChanges();

                    ctx.Logger.Debug($"Created record with id {newItem.id} for msg id {message.MessageId}");

                    outLogEntry(ObjectExt.Clone(newItem));
                }
                catch (Exception e)
                {
                    throw new Exception($"Could not write message {message.MessageId} to store", e);
                }
            }

            static async Task FindLogEntry(ContextWithMessage ctx, Action<MessageLogEntry> outResult)
            {
                var message = ctx.Message;

                try
                {
                    ctx.Logger.Debug($"Looking for msg id {message.MessageId}");

                    var result = await ctx.DataStore.ReadActiveById<MessageLogEntry>(message.MessageId);

                    ctx.Logger.Debug(
                        result == null
                            ? $"Failed to find record for msg id {message.MessageId}"
                            : $"Found record with id {result.id} for msg with id {message.MessageId}");

                    outResult(result);
                }
                catch (Exception e)
                {
                    throw new Exception($"Could not read message {message.MessageId} from store", e);
                }
            }
        }

        internal static ContextWithMessageLogEntry Upgrade(this ContextWithMessage current, MessageLogEntry messageLogEntry) =>
            new ContextWithMessageLogEntry(messageLogEntry, current);
    }
}