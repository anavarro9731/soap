namespace Palmtree.Api.Sso.Endpoint.Http.Controllers
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.MessagePipeline;
    using Soap.Pf.HttpEndpointBase.Controllers;

    public class StatefulProcessController : BaseController
    {
        public StatefulProcessController(IApplicationConfig applicationConfig, MessagePipeline messagePipeline)
            : base(applicationConfig, messagePipeline)
        {
        }

        [HttpPost]
        [Route("sp/{spAction}/{processId}")]
        public async void LoadProcess(string spAction, Guid processId)
        {
            switch (spAction)
            {
                case "confirmemail":
                    await this.MessagePipeline.Execute(new ConfirmEmail(processId)).ConfigureAwait(false);
                    break;
            }
        }
    }
}