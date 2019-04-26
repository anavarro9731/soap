﻿namespace Soap.Api.Sample.Endpoint.Http.Handlers.Queries.Abstract
{
    using Soap.Api.Sample.Domain.Messages.Queries.Abstract;
    using Soap.If.Interfaces;
    using Soap.Pf.HttpEndpointBase.Handlers.Queries;

    public class GetMessageLogItemHandler : AbstractGetMessageLogItemHandler<GetMessageLogItemQuery,GetMessageLogItemQuery.GetMessageLogItemResponse>
    {
        public GetMessageLogItemHandler(IApplicationConfig applicationConfig)
            : base(applicationConfig)
        {
        }
    }
}
