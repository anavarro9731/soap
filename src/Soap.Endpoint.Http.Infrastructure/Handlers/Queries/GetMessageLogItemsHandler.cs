namespace Soap.Pf.HttpEndpointBase.Handlers.Queries
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.If.Utility.PureFunctions.Extensions;
    using Soap.Pf.ClientServerMessaging.Queries;

    public class GetMessageLogItemsHandler : QueryHandler<GetMessageLogItemsQuery, GetMessageLogItemsQuery.ResponseModel>
    {
        private readonly IApplicationConfig applicationConfig;

        public GetMessageLogItemsHandler(IApplicationConfig applicationConfig)
        {
            this.applicationConfig = applicationConfig;
        }

        protected override Task<GetMessageLogItemsQuery.ResponseModel> Handle(GetMessageLogItemsQuery query, ApiMessageMeta meta)
        {
            {
                Validate();

                var results = query.MessageIdsOfLogItems.Select(msgId => DataStore.ReadActiveById<MessageLogItem>(msgId).Result)
                                   .Select(msgLogItem => ToViewModel(msgLogItem, this.applicationConfig))
                                   .ToArray();

                var response = results.Map(
                    r => new GetMessageLogItemsQuery.ResponseModel
                    {
                        MessageLogItems = results
                    });

                return Task.FromResult(response);
            }

            void Validate()
            {
                GetMessageLogItemsQueryValidator.Default.ValidateAndThrow(query);
            }
        }

        private static GetMessageLogItemsQuery.MessageLogItemViewModel ToViewModel(MessageLogItem message, IApplicationConfig applicationConfig)
        {
            if (message == null) return null;

            return new GetMessageLogItemsQuery.MessageLogItemViewModel
            {
                ClrTypeOfMessage = message.ClrTypeOfMessage,
                Message = message.Message,
                SuccessfulAttempt = ToViewModel(message.SuccessfulAttempt),
                FailedAttempts = message.FailedAttempts.Select(failure => ToViewModel(failure, applicationConfig)).ToList(),
                MaxFailedMessages = message.MaxFailedMessages
            };
        }

        private static GetMessageLogItemsQuery.MessageLogItemViewModel.SuccessMessageResult ToViewModel(MessageLogItem.SuccessMessageResult success)
        {
            if (success == null) return null;

            return new GetMessageLogItemsQuery.MessageLogItemViewModel.SuccessMessageResult
            {
                ReturnValue = success.ReturnValue,
                SucceededAt = success.SucceededAt
            };
        }

        private static GetMessageLogItemsQuery.MessageLogItemViewModel.FailedMessageResult ToViewModel(
            MessageLogItem.FailedMessageResult failure,
            IApplicationConfig applicationConfig)
        {
            if (failure == null) return null;

            return new GetMessageLogItemsQuery.MessageLogItemViewModel.FailedMessageResult
            {
                Error = failure.Errors.ToEnvironmentSpecificError(applicationConfig),
                FailedAt = failure.FailedAt
            };
        }
    }
}