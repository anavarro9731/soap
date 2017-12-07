namespace Soap.Pf.HttpEndpointBase.Handlers.Queries
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.Pf.ClientServerMessaging.Queries;
    using Soap.Pf.EndpointInfrastructure;

    public class GetMessageLogItemHandler : QueryHandler<GetMessageLogItemQuery, GetMessageLogItemQuery.ResponseModel>
    {
        private readonly IApplicationConfig applicationConfig;

        public GetMessageLogItemHandler(IApplicationConfig applicationConfig)
        {
            this.applicationConfig = applicationConfig;
        }

        protected override async Task<GetMessageLogItemQuery.ResponseModel> Handle(GetMessageLogItemQuery query, ApiMessageMeta meta)
        {
            {
                Validate();

                var msgLogItem = await DataStore.ReadActiveById<MessageLogItem>(query.MessageIdOfLogItem);

                var viewModel = ToViewModel(msgLogItem, this.applicationConfig);

                return viewModel;
            }

            void Validate()
            {
                GetMessageLogItemQueryValidator.Default.ValidateAndThrow(query);
            }
        }

        private static GetMessageLogItemQuery.ResponseModel ToViewModel(MessageLogItem message, IApplicationConfig applicationConfig)
        {
            if (message == null) return null;

            return new GetMessageLogItemQuery.ResponseModel
            {
                ClrTypeOfMessage = message.ClrTypeOfMessage,
                Message = message.Message,
                SuccessfulAttempt = ToViewModel(message.SuccessfulAttempt),
                FailedAttempts = message.FailedAttempts.Select(failure => ToViewModel(failure, applicationConfig)).ToList(),
                MaxFailedMessages = message.MaxFailedMessages
            };
        }

        private static GetMessageLogItemQuery.ResponseModel.SuccessMessageResult ToViewModel(MessageLogItem.SuccessMessageResult success)
        {
            if (success == null) return null;

            return new GetMessageLogItemQuery.ResponseModel.SuccessMessageResult
            {
                ReturnValue = success.ReturnValue,
                SucceededAt = success.SucceededAt
            };
        }

        private static GetMessageLogItemQuery.ResponseModel.FailedMessageResult ToViewModel(
            MessageLogItem.FailedMessageResult failure,
            IApplicationConfig applicationConfig)
        {
            if (failure == null) return null;

            return new GetMessageLogItemQuery.ResponseModel.FailedMessageResult
            {
                Error = failure.Errors.ToEnvironmentSpecificError(applicationConfig),
                FailedAt = failure.FailedAt
            };
        }
    }
}