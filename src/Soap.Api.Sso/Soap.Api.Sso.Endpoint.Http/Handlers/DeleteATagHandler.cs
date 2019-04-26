namespace Soap.Api.Sso.Endpoint.Http.Handlers
{
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Logic.Operations;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.HttpEndpointBase;

    public class DeleteATagHandler : CommandHandler<DeleteTag, Tag>
    {
        private readonly TagOperations TagOperations;

        public DeleteATagHandler(TagOperations TagOperations)
        {
            this.TagOperations = TagOperations;
        }

        protected override async Task<Tag> Handle(DeleteTag message, ApiMessageMeta meta)
        {
            return await this.TagOperations.RemoveTag(message);
        }
    }
}