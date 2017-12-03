namespace Soap.Endpoint.Http.Infrastructure.Handlers.Queries
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.AbstractMessages.Queries;
    using Soap.Interfaces;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Models;
    using Soap.MessagePipeline.Models.Aggregates;

    public class GetMessageLogItemHandler : MessageHandler<GetMessageLogItemQuery, GetMessageLogItemQuery.MessageLogItemViewModel>
    {
        private readonly IApplicationConfig applicationConfig;

        public GetMessageLogItemHandler(IApplicationConfig applicationConfig)
        {
            this.applicationConfig = applicationConfig;
        }

        protected override async Task<GetMessageLogItemQuery.MessageLogItemViewModel> Handle(GetMessageLogItemQuery query, ApiMessageMeta meta)
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

        private static GetMessageLogItemQuery.MessageLogItemViewModel ToViewModel(MessageLogItem message, IApplicationConfig applicationConfig)
        {
            if (message == null) return null;

            return new GetMessageLogItemQuery.MessageLogItemViewModel
            {
                ClrTypeOfMessage = message.ClrTypeOfMessage,
                Message = message.Message,
                SuccessfulAttempt = ToViewModel(message.SuccessfulAttempt),
                FailedAttempts = message.FailedAttempts.Select(failure => ToViewModel(failure, applicationConfig)).ToList(),
                MaxFailedMessages = message.MaxFailedMessages
            };
        }

        private static GetMessageLogItemQuery.MessageLogItemViewModel.SuccessMessageResult ToViewModel(MessageLogItem.SuccessMessageResult success)
        {
            if (success == null) return null;

            return new GetMessageLogItemQuery.MessageLogItemViewModel.SuccessMessageResult
            {
                ReturnValue = success.ReturnValue,
                SucceededAt = success.SucceededAt
            };
        }

        private static GetMessageLogItemQuery.MessageLogItemViewModel.FailedMessageResult ToViewModel(
            MessageLogItem.FailedMessageResult failure,
            IApplicationConfig applicationConfig)
        {
            if (failure == null) return null;

            return new GetMessageLogItemQuery.MessageLogItemViewModel.FailedMessageResult
            {
                Error = failure.Errors.ToEnvironmentSpecificError(applicationConfig),
                FailedAt = failure.FailedAt
            };
        }
    }
}