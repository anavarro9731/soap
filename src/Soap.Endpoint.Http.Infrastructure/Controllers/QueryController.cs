namespace Soap.Endpoint.Http.Infrastructure.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.MessagePipeline;

    public class QueryController : BaseController
    {
        private readonly CachedSchema cachedSchema;

        public QueryController(IApplicationConfig applicationConfig, MessagePipeline messagePipeline, IList<MessageHandler> handlers)
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

        private CachedSchema BuildQuerySchema(IList<MessageHandler> handlers)
        {
            return CachedSchema.Create<IApiQuery>(this.ApplicationConfig, handlers);
        }
    }
}