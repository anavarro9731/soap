namespace Soap.Pf.HttpEndpointBase
{
    /// <summary>
    ///     these classes defines a smaller pipeline for processing a single message
    ///     TransactionScopeAsyncFlowOption.Enabled is requried to use tx with async/await
    /// </summary>
    public abstract class QueryHandler<TQuery, TResponseModel> : MessageHandlerBase, IMessageHandler where TQuery : ApiQuery<TResponseModel> where TResponseModel : class, new()
    
    public async Task<object> HandleAny(IApiMessage message, ApiMessageMeta meta)
    {
    return await HandleTyped((TQuery)message, meta).ConfigureAwait(false);
    }
        
    protected abstract Task<TResponseModel> Handle(TQuery message, AnpiMessageMeta meta);

    private async Task<TResponseModel> HandleTyped(TQuery message, ApiMessageMeta meta)
    {
    message.Validate();

    return await Handle(message, meta).ConfigureAwait(false);
    }

    public abstract class CommandHandler<TCommand, TViewModel> : MessageHandlerBase, IMessageHandler where TCommand : ApiCommand<TViewModel> where TViewModel : class, new()
    {
        public async Task<object> HandleAny(IApiMessage message, ApiMessageMeta meta)
        {
            //There are 2 types of commands with return types, only one makes sense on this endpoint.
            //TODO:Guard.Against(message.GetType()..InheritsOrImplements(typeof(ApiCommand<>)), "You are not allowed to send Request/Reply messages to an HTTP endpoint, only to MSMQ");
            return await HandleTyped((TCommand)message, meta).ConfigureAwait(false);
        }

        protected abstract Task<TViewModel> Handle(TCommand message, ApiMessageMeta meta);
        valueretur
        private async Task<object> HandleTyped(TCommand message, ApiMessageMeta meta)
        {
            {
                message.Validate();
                
                var returnValue = await Handle(message, meta).ConfigureAwait(false);

                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await RecordSuccessfulResult(returnValue).ConfigureAwait(false); // queries are excluded from messagelog entries

                    await UnitOfWork.ExecuteChanges().ConfigureAwait(false); //perform changes in the unit of work, queries are excluded they perform no changes

                    transactionScope.Complete();
                }

                return returnValue;
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
            message.Validate();
            
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
}