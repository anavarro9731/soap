namespace Soap.Api.Sample.Endpoint.Http.Controllers
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.MessagePipeline;
    using Soap.Pf.HttpEndpointBase.Controllers;

    public class StatefulProcessController : BaseController
    {
        //TODO upgrades here
        public StatefulProcessController(IApplicationConfig applicationConfig, MessagePipeline messagePipeline)
            : base(applicationConfig, messagePipeline)
        {
        }

        [HttpPost]
        [Route("sp/{spAction}/{processId}")]
        public void LoadProcess(string spAction, Guid processId)
        {
            switch (spAction)

            {
            }
        }
    }
}