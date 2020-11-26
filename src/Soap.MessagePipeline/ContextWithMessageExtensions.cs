// -----------------------------------------------------------------------
// <copyright file="$FILENAME$" company="$COMPANYNAME$">
// $COPYRIGHT$
// </copyright>
// <summary>
// $SUMMARY$
// </summary>


namespace Soap.MessagePipeline
{
    using System;
    using System.Threading.Tasks;
    using Soap.Context.Context;
    using Soap.Context.Logging;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    internal static class ContextWithMessageExtensions
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
                    ctx.Logger.Debug($"Creating record for msg id {message.Headers.GetMessageId()}");

                    var messageLogEntry = new MessageLogEntry(
                        message,
                        meta,
                        ctx.DataStore.DataStoreOptions.OptimisticConcurrency, //* determines how uow will behave
                        ctx.Bus.MaximumNumberOfRetries);

                    var newItem = await ctx.DataStore.Create(messageLogEntry);

                    await ctx.DataStore.CommitChanges();

                    ctx.Logger.Debug($"Created record with id {newItem.id} for msg id {message.Headers.GetMessageId()}");

                    outLogEntry(newItem.Clone());
                }
                catch (Exception e)
                {
                    throw new ApplicationException($"Could not write message {message.Headers.GetMessageId()} to store", e);
                }
            }

            static async Task FindLogEntry(ContextWithMessage ctx, Action<MessageLogEntry> outResult)
            {
                var message = ctx.Message;

                try
                {
                    ctx.Logger.Debug($"Looking for msg id {message.Headers.GetMessageId()}");

                    var result = await ctx.DataStore.ReadActiveById<MessageLogEntry>(message.Headers.GetMessageId());

                    ctx.Logger.Debug(
                        result == null
                            ? $"Failed to find record for msg id {message.Headers.GetMessageId()}"
                            : $"Found record with id {result.id} for msg with id {message.Headers.GetMessageId()}");

                    outResult(result);
                }
                catch (Exception e)
                {
                    throw new ApplicationException($"Could not read message {message.Headers.GetMessageId()} from store", e);
                }
            }
        }

    }
}

