namespace Soap.Pf.HttpEndpointBase.Handlers.Queries
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.Pf.MessageContractsBase.Queries;

    public class AbstractGetMessageLogItemHandler<TQuery, TResponse> : QueryHandler<TQuery, TResponse>
        where TQuery : AbstractGetMessageLogItemQuery<TResponse>, new() where TResponse : AbstractGetMessageLogItemQuery<TResponse>.AbstractResponseModel, new()
    {
        private readonly IApplicationConfig applicationConfig;

        public AbstractGetMessageLogItemHandler(IApplicationConfig applicationConfig)
        {
            this.applicationConfig = applicationConfig;
        }

        protected override async Task<TResponse> Handle(TQuery query, ApiMessageMeta meta)
        {
            {
                var msgLogItem = await DataStore.ReadActiveById<MessageLogItem>(query.MessageIdOfLogItem);

                var viewModel = ToViewModel(msgLogItem, this.applicationConfig);

                return viewModel;
            }

        }

        private static TResponse ToViewModel(MessageLogItem message, IApplicationConfig applicationConfig)
        {
            if (message == null) return null;

            return new TResponse
            {
                ClrTypeOfMessage = message.ClrTypeOfMessage,
                Message = message.Message,
                SuccessfulAttempt = ToViewModel(message.SuccessfulAttempt),
                FailedAttempts = message.FailedAttempts.Select(failure => ToViewModel(failure, applicationConfig)).ToList(),
                MaxFailedMessages = message.MaxFailedMessages
            };
        }

        private static AbstractGetMessageLogItemQuery<TResponse>.AbstractResponseModel.AbstractSuccessMessageResult ToViewModel(MessageLogItem.SuccessMessageResult success)
        {
            if (success == null) return null;

            return new AbstractGetMessageLogItemQuery<TResponse>.AbstractResponseModel.AbstractSuccessMessageResult()
            {
                ReturnValue = success.ReturnValue, SucceededAt = success.SucceededAt
            };
        }

        private static AbstractGetMessageLogItemQuery<TResponse>.AbstractResponseModel.AbstractFailedMessageResult ToViewModel(
            MessageLogItem.FailedMessageResult failure,
            IApplicationConfig applicationConfig)
        {
            if (failure == null) return null;

            return new AbstractGetMessageLogItemQuery<TResponse>.AbstractResponseModel.AbstractFailedMessageResult()
            {
                Error = failure.Errors.ToEnvironmentSpecificError(applicationConfig), FailedAt = failure.FailedAt
            };
        }
    }
}