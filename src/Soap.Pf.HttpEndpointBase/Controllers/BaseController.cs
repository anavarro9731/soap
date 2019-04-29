namespace Soap.Pf.HttpEndpointBase.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.MessagePipeline;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;

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
                    //TODO when this is returned to endpoint client on failure the error is lost
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