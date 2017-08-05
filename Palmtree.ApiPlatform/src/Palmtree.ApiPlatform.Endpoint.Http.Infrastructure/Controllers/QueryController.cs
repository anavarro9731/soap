namespace Palmtree.ApiPlatform.Endpoint.Http.Infrastructure.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Interfaces;
    using Palmtree.ApiPlatform.MessagePipeline;
    using Palmtree.ApiPlatform.MessagePipeline.MessagePipeline;

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
