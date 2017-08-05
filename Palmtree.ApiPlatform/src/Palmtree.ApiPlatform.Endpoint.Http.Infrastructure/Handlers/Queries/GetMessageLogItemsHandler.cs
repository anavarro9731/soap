﻿namespace Palmtree.ApiPlatform.Endpoint.Http.Infrastructure.Handlers.Queries
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentValidation;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Interfaces;
    using Palmtree.ApiPlatform.MessagesSharedWithClients.Queries;

    public class GetMessageLogItemsHandler : MessageHandler<GetMessageLogItemsQuery, IEnumerable<GetMessageLogItemsQuery.MessageLogItemViewModel>>
    {
        private readonly IApplicationConfig applicationConfig;

        public GetMessageLogItemsHandler(IApplicationConfig applicationConfig)
        {
            this.applicationConfig = applicationConfig;
        }

        protected override Task<IEnumerable<GetMessageLogItemsQuery.MessageLogItemViewModel>> Handle(GetMessageLogItemsQuery query, ApiMessageMeta meta)
        {
            {
                Validate();

                var results = query.MessageIdsOfLogItems.Select(msgId => DataStore.ReadActiveById<MessageLogItem>(msgId).Result)
                                   .Select(msgLogItem => ToViewModel(msgLogItem, this.applicationConfig))
                                   .ToList()
                                   .AsEnumerable();

                return Task.FromResult(results);
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
