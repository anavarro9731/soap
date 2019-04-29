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

    public class CommandController : BaseController
    {
        private readonly CachedSchema cachedSchema;

        public CommandController(IApplicationConfig applicationConfig, MessagePipeline messagePipeline, IList<IMessageHandler> handlers)
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

        private CachedSchema BuildCommandSchema(IList<IMessageHandler> handlers)
        {
            return CachedSchema.Create<IApiCommand>(this.ApplicationConfig, handlers);
        }
    }
}