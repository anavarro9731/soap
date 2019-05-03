namespace Soap.Pf.HttpEndpointBase.Controllers
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.MessagePipeline;
    using Soap.Pf.EndpointInfrastructure;

    public class StatefulProcessController : BaseController
    {
        private readonly MessageInstanceCreator messageInstanceCreator;

        public StatefulProcessController(
            IApplicationConfig applicationConfig,
            MessagePipeline messagePipeline,
            MessageInstanceCreator messageInstanceCreator)
            : base(applicationConfig, messagePipeline)
        {
            this.messageInstanceCreator = messageInstanceCreator;
        }

        [HttpPost("sp/{messageType}/{processId}/{identityToken?}",Name = nameof(LoadProcess))]
        public async void LoadProcess(string messageType, Guid processId, string identityToken)
        {
            var message = this.messageInstanceCreator.CreateInstance(messageType);
            if (message is IApiCommand command)
            {
                command.IdentityToken = identityToken;
                command.StatefulProcessId = processId;
                await this.MessagePipeline.Execute(command).ConfigureAwait(false);
            }
        }
    }
}