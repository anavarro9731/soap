using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Palmtree.Api.Sso.Endpoint.Http.Handlers.Queries.Abstract
{
    using Palmtree.Api.Sso.Domain.Messages.Queries.Abstract;
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
