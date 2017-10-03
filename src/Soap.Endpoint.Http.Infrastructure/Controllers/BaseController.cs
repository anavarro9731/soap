namespace Soap.Endpoint.Http.Infrastructure.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Soap.Interfaces;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.Utility.PureFunctions;
    using Soap.Utility.PureFunctions.Extensions;

    public abstract class BaseController : Controller
    {
        protected readonly IApplicationConfig ApplicationConfig;

        protected readonly MessagePipeline MessagePipeline;

        protected BaseController(IApplicationConfig applicationConfig, MessagePipeline messagePipeline)
        {
            this.ApplicationConfig = applicationConfig;
            this.MessagePipeline = messagePipeline;
        }

        protected async Task<IActionResult> HandleMessage<TApiMessage>(JObject message) where TApiMessage : IApiMessage
        {
            try
            {
                var apiMessage = message.ToObject();

                Guard.Against(!(apiMessage is TApiMessage), $"'{apiMessage?.GetType().FullName ?? "???"}' API message must be of type '{typeof(TApiMessage)}'");

                try
                {
                    var result = await this.MessagePipeline.Execute((TApiMessage)apiMessage).ConfigureAwait(false);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return BadRequest(
                        new
                        {
                            Error = ex.Message
                        });
                }
            }
            catch (Exception ex)
            {
                var error = new
                {
                    Error = this.ApplicationConfig.ReturnExplicitErrorMessages ? ex.ToString() : ex.Message
                };
                return BadRequest(error);
            }
        }
    }
}
