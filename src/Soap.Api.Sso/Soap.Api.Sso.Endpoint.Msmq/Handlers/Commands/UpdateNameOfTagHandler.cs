namespace Soap.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Logic.Operations;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.MsmqEndpointBase;

    public class UpdateNameOfTagHandler : CommandHandler<UpdateNameOfTag>
    {
        private readonly TagOperations TagOperations;

        public UpdateNameOfTagHandler(TagOperations TagOperations)
        {
            this.TagOperations = TagOperations;
        }

        protected override async Task Handle(UpdateNameOfTag message, ApiMessageMeta meta)
        {
            await this.TagOperations.UpdateName(message);
        }
    }
}