namespace Palmtree.Sample.Api.Endpoint.Http.Controllers
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Palmtree.ApiPlatform.Endpoint.Http.Infrastructure.Controllers;
    using Palmtree.ApiPlatform.Interfaces;
    using Palmtree.ApiPlatform.MessagePipeline.MessagePipeline;
    using Palmtree.Sample.Api.Domain.Messages.Commands;

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
