﻿namespace Soap.Api.Sso.Endpoint.Http.Handlers.Queries
{
    using Soap.Api.Sso.Domain.Messages.Queries;
    using Soap.If.Interfaces;
    using Soap.Pf.HttpEndpointBase.Handlers.Queries;

    public class GetMessageLogItemHandler : AbstractGetMessageLogItemHandler<GetMessageLogItemQuery, GetMessageLogItemQuery.GetMessageLogItemResponse>
    {
        public GetMessageLogItemHandler(IApplicationConfig applicationConfig)
            : base(applicationConfig)
        {
        }
    }
}