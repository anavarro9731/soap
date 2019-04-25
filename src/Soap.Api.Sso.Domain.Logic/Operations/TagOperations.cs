namespace Soap.Api.Sso.Domain.Logic.Operations
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.If.MessagePipeline.ProcessesAndOperations;

    public class TagOperations : Operations<Tag>
    {
        public Task<Tag> AddTag(Guid? id, string nameOfTag)
        {
            {
                DetermineChange(out var Tag);

                return DataStore.Create(Tag);
            }

            void DetermineChange(out Tag Tag)
            {
                Tag = new Tag
                {
                    id = id ?? Guid.NewGuid(),
                    NameOfTag = nameOfTag
                };
            }
        }

        public Task<Tag> RemoveTag(DeleteTag msg)
        {
            return DataStore.DeleteSoftById(msg.TagId);
        }

        public Task<Tag> UpdateName(UpdateNameOfTag msg)
        {
            {
                DetermineChange(out var change, msg.NameOfTag);

                return DataStore.UpdateById(msg.TagId, change);
            }

            void DetermineChange(out Action<Tag> change, string nameOfTag)
            {
                change = t => t.NameOfTag = nameOfTag;
            }
        }
    }
}