namespace Soap.Api.Sample.Domain.Messages.Queries
{
    using Soap.Pf.MessageContractsBase.Queries;

    public sealed class GetMessageLogItemQuery : AbstractGetMessageLogItemQuery<GetMessageLogItemQuery.GetMessageLogItemResponse>
    {
        public sealed class GetMessageLogItemResponse : AbstractResponseModel
        {
            public sealed class FailedMessageViewModel : AbstractFailedMessageResult
            {
            }

            public sealed class SuccessMessageViewModel : AbstractSuccessMessageResult
            {
            }
        }
    }
}