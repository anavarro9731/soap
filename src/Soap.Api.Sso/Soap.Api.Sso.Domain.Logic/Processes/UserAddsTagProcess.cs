namespace Soap.Api.Sso.Domain.Logic.Processes
{
    using System.Threading.Tasks;
    using CircuitBoard.Permissions;
    using Soap.Api.Sso.Domain.Logic.Operations;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.ProcessesAndOperations;

    public class UserAddsTagProcess : Process<UserAddsTagProcess>, IBeginProcess<AddATag, Tag>
    {
        private readonly TagOperations TagOperations;

        private readonly UserOperations userOperations;

        public UserAddsTagProcess(TagOperations TagOperations, UserOperations userOperations)
        {
            this.TagOperations = TagOperations;
            this.userOperations = userOperations;
        }

        public async Task<Tag> BeginProcess(AddATag message, ApiMessageMeta meta)
        {
            {
                var newTag = await AddTagToDatabase();

                await AddTagToUser(newTag, meta.RequestedBy);

                return newTag;
            }

            async Task<Tag> AddTagToDatabase()
            {
                return await this.TagOperations.AddTag(message.TagId, message.NameOfTag);
            }

            async Task AddTagToUser(Tag Tag, IUserWithPermissions requestedBy)
            {
                await this.userOperations.AddTagToUser(requestedBy.id, Tag);
            }
        }
    }
}