namespace Palmtree.ApiPlatform.Infrastructure
{
    using System.Threading.Tasks;
    using System.Transactions;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Infrastructure.PureFunctions;

    /// <summary>
    ///     these classes defines a smaller pipeline for processing a single message
    ///     TransactionScopeAsyncFlowOption.Enabled is requried to use tx with async/await
    /// </summary>
    public abstract class MessageHandler : ApiMessageContext
    {
        public abstract Task<object> HandleAny(IApiMessage message, ApiMessageMeta meta);

        protected async Task TimingHack(IApiMessage message)
        {
            //HACK: sometimes the suppressed transaction which records the messagelogitem to begin with
            //in MessagePipeline.StateChangingMessageConstraints while committed is not visible to the ambient txn yet
            //it seems rare but this is the only fix we could think of
            var attempts = 8;
            var millisecondsDelay = 250;

            for (var j = 1; j <= attempts; j++)
            {
                var exists = await DataStore.Exists(message.MessageId).ConfigureAwait(false);
                if (exists) break;
                Logger.Warning(
                    $"MessgeLogItem record for message with id {message.MessageId} not found while attempting to update it with a successful result. Pausing for {250}ms and will try again. {attempts - j} tries remaining.");
                await Task.Delay(millisecondsDelay).ConfigureAwait(false);
            }
        }
    }

    public abstract class MessageHandler<TMessage> : MessageHandler where TMessage : IApiMessage
    {
        public override async Task<object> HandleAny(IApiMessage message, ApiMessageMeta meta)
        {
            await HandleTyped((TMessage)message, meta).ConfigureAwait(false);
            return null;
        }

        protected abstract Task Handle(TMessage message, ApiMessageMeta meta);

        private async Task HandleTyped(TMessage message, ApiMessageMeta meta)
        {
            //tx needed for messages which can change state sent via http with no ambient trans
            //and for any state changes not done via the uow but enlisted in a .net txn (e.g. ADO.NET calls in legacy code)
            //don't forget there may still be an outer transaction scope when the pipeline is run from a messaging service

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await Handle(message, meta).ConfigureAwait(false);

                await RecordSuccessfulResult().ConfigureAwait(false);

                await UnitOfWork.ExecuteChanges().ConfigureAwait(false); //perform changes in the unit of work

                scope.Complete();
            }

            async Task RecordSuccessfulResult()
            {
                await TimingHack(message).ConfigureAwait(false);

                //logs inside the txn so it will write only if the parent succeeds
                await DataStore.UpdateById<MessageLogItem>(message.MessageId, obj => MessageLogItemOperations.AddSuccessfulMessageResult(obj)).ConfigureAwait(false);
            }
        }
    }

    public abstract class MessageHandler<TMessage, TReturnType> : MessageHandler where TMessage : IApiMessage where TReturnType : class
    {
        public override async Task<object> HandleAny(IApiMessage message, ApiMessageMeta meta)
        {
            return await HandleTyped((TMessage)message, meta).ConfigureAwait(false);
        }

        protected abstract Task<TReturnType> Handle(TMessage message, ApiMessageMeta meta);

        private async Task<TReturnType> HandleTyped(TMessage message, ApiMessageMeta meta)
        {
            {
                //tx needed for messages which can change state sent via http with no ambient trans
                //and for any state changes not done via the uow but enlisted in a .net txn (e.g. ADO.NET calls in legacy code)
                //don't forget there may still be an outer transaction scope when the pipeline is run from a messaging service

                TransactionScope scope = null;
                try
                {
                    if (message.CanChangeState()) //exclude queries from txns to avoid deadlocks
                    {
                        scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                    }
                    var returnValue = await Handle(message, meta).ConfigureAwait(false);

                    if (message.CanChangeState())
                    {
                        await RecordSuccessfulResult(returnValue).ConfigureAwait(false); // queries are excluded from messagelog entries

                        await UnitOfWork.ExecuteChanges().ConfigureAwait(false); //perform changes in the unit of work, queries are excluded they perform no changes

                        scope.Complete();
                    }

                    return returnValue;
                }
                finally
                {
                    scope?.Dispose();
                }
            }

            async Task RecordSuccessfulResult(object returnValue)
            {
                await TimingHack(message).ConfigureAwait(false);

                await DataStore.UpdateById<MessageLogItem>(message.MessageId, obj => MessageLogItemOperations.AddSuccessfulMessageResult(obj, returnValue))
                               .ConfigureAwait(false);
            }
        }
    }
}
