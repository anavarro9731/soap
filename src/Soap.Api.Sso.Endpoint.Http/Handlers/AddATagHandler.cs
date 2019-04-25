namespace Soap.Api.Sso.Endpoint.Http.Handlers
{
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Logic.Processes;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.Pf.HttpEndpointBase;

    public class AddATagHandler : CommandHandler<AddATag, Tag>
    {
        private readonly IProcess<UserAddsTagProcess> process;

        public AddATagHandler(IProcess<UserAddsTagProcess> process)
        {
            this.process = process;
        }

        protected override async Task<Tag> Handle(AddATag message, ApiMessageMeta meta)
        {
            return await this.process.BeginProcess<AddATag, Tag>(message, meta);
        }
    }
}