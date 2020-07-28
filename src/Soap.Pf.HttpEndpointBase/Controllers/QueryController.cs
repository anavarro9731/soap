namespace Soap.Pf.HttpEndpointBase.Controllers
{
    public class QueryController : BaseController
    {
        private readonly CachedSchema cachedSchema;

        public QueryController(
            IApplicationConfig applicationConfig,
            MessagePipeline messagePipeline,
            IList<IMessageHandler> handlers)
            : base(applicationConfig, messagePipeline)
        {
            this.cachedSchema = BuildQuerySchema(handlers);
        }

        [HttpGet("query/schema", Name = nameof(GetQuerySchema))]
        public string GetQuerySchema() => this.cachedSchema.Schema;

        [HttpPost("query", Name = nameof(PostQuery))]
        public async Task<IActionResult> PostQuery([FromBody] JObject message) =>
            await HandleMessage<IApiQuery>(message).ConfigureAwait(false);

        private CachedSchema BuildQuerySchema(IList<IMessageHandler> handlers) =>
            CachedSchema.Create<IApiQuery>(this.ApplicationConfig, handlers);
    }
}