namespace Soap.Endpoint.Http.Infrastructure.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Soap.Interfaces;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.MessagePipeline;

    public class CommandController : BaseController
    {
        private readonly CachedSchema cachedSchema;

        public CommandController(IApplicationConfig applicationConfig, MessagePipeline messagePipeline, IList<MessageHandler> handlers)
            : base(applicationConfig, messagePipeline)
        {
            this.cachedSchema = BuildCommandSchema(handlers);
        }

        [HttpGet("command/schema", Name = nameof(GetCommandSchema))]
        public string GetCommandSchema()
        {
            return this.cachedSchema.Schema;
        }

        [HttpPost("command", Name = nameof(PostCommand))]
        public async Task<IActionResult> PostCommand([FromBody] JObject message)
        {
            return await HandleMessage<IApiCommand>(message).ConfigureAwait(false);
        }

        private CachedSchema BuildCommandSchema(IList<MessageHandler> handlers)
        {
            return CachedSchema.Create<IApiCommand>(this.ApplicationConfig, handlers);
        }
    }
}
