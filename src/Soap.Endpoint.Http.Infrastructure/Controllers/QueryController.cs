namespace Soap.Pf.HttpEndpointBase.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline;
    using Soap.If.MessagePipeline.MessagePipeline;
    using Soap.Pf.EndpointInfrastructure;

    public class QueryController : BaseController
    {
        private readonly CachedSchema cachedSchema;

        public QueryController(IApplicationConfig applicationConfig, MessagePipeline messagePipeline, IList<IMessageHandler> handlers)
            : base(applicationConfig, messagePipeline)
        {
            this.cachedSchema = BuildQuerySchema(handlers);
        }

        [HttpGet("query/schema", Name = nameof(GetQuerySchema))]
        public string GetQuerySchema()
        {
            return this.cachedSchema.Schema;
        }

        [HttpPost("query", Name = nameof(PostQuery))]
        public async Task<IActionResult> PostQuery([FromBody] JObject message)
        {
            return await HandleMessage<IApiQuery>(message).ConfigureAwait(false);
        }

        private CachedSchema BuildQuerySchema(IList<IMessageHandler> handlers)
        {
            return CachedSchema.Create<IApiQuery>(this.ApplicationConfig, handlers);
        }
    }
}