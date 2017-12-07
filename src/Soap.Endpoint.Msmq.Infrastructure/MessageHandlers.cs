namespace Soap.Pf.MsmqEndpointBase
{
    using System.Threading.Tasks;
    using System.Transactions;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.Pf.EndpointInfrastructure;

    /// <summary>
    ///     these classes defines a smaller pipeline for processing a single message
    ///     TransactionScopeAsyncFlowOption.Enabled is requried to use tx with async/await
    /// </summary>
    public abstract class CommandHandler<TCommand, R> : MessageHandlerBase, IMessageHandler where TCommand : ApiCommand<R> where R : class, IApiCommand, new()
    {
        public async Task<object> HandleAny(IApiMessage message, ApiMessageMeta meta)
        {
            //There are 2 types of commands with return types, only one makes sense on this endpoint.
            //TODO:Guard.Against(message.GetType()..InheritsOrImplements(typeof(ApiCommand<>)), "You are not allowed to send Request/Reply messages to an HTTP endpoint, only to MSMQ");

            await HandleTyped((TCommand)message, meta).ConfigureAwait(false);
            return null;
        }

        protected abstract Task<R> Handle(TCommand message, ApiMessageMeta meta);

        private async Task HandleTyped(TCommand message, ApiMessageMeta meta)
        {
            {
                var reply = await Handle(message, meta).ConfigureAwait(false);

                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await RecordSuccessfulResult(reply).ConfigureAwait(false); // queries are excluded from messagelog entries

                    //TODO: verify reply
                    UnitOfWork.SendCommand(reply);

                    await UnitOfWork.ExecuteChanges().ConfigureAwait(false); //perform changes in the unit of work, queries are excluded they perform no changes

                    transactionScope.Complete();
                }
            }

            async Task RecordSuccessfulResult(object returnValue)
            {
                //log inside the txn so it will write only if the parent succeeds
                await DataStore.UpdateById<MessageLogItem>(message.MessageId, obj => MessageLogItemOperations.AddSuccessfulMessageResult(obj, returnValue))
                               .ConfigureAwait(false);
            }
        }
    }

    public abstract class CommandHandler<TCommand> : MessageHandlerBase, IMessageHandler where TCommand : ApiCommand
    {
        public async Task<object> HandleAny(IApiMessage message, ApiMessageMeta meta)
        {
            await HandleTyped((TCommand)message, meta).ConfigureAwait(false);
            return null;
        }

        protected abstract Task Handle(TCommand message, ApiMessageMeta meta);

        private async Task HandleTyped(TCommand message, ApiMessageMeta meta)
        {
            await Handle(message, meta).ConfigureAwait(false);

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await RecordSuccessfulResult().ConfigureAwait(false); // queries are excluded from messagelog entries

                await UnitOfWork.ExecuteChanges().ConfigureAwait(false); //perform changes in the unit of work, queries are excluded they perform no changes

                transactionScope.Complete();
            }

            async Task RecordSuccessfulResult()
            {
                //log inside the txn so it will write only if the parent succeeds
                await DataStore.UpdateById<MessageLogItem>(message.MessageId, logItem => MessageLogItemOperations.AddSuccessfulMessageResult(logItem))
                               .ConfigureAwait(false);
            }
        }
    }
}