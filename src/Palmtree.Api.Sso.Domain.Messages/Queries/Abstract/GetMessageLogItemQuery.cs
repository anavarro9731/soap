namespace Palmtree.Api.Sso.Domain.Messages.Queries.Abstract
{
    using Soap.Pf.ClientServerMessaging.Queries;

    public sealed class GetMessageLogItemQuery : AbstractGetMessageLogItemQuery<GetMessageLogItemQuery.GetMessageLogItemResponse>
    {
        public sealed class GetMessageLogItemResponse : AbstractResponseModel
        {
            public sealed class FailedMessageResult : AbstractResponseModel.AbstractFailedMessageResult
            {
            }

            public sealed class SuccessMessageResult : AbstractResponseModel.AbstractSuccessMessageResult
            {
            }
        }
    }
}