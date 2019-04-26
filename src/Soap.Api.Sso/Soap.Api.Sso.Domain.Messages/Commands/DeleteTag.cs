namespace Soap.Api.Sso.Domain.Messages.Commands
{
    using System;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.If.Interfaces.Messages;

    public class DeleteTag : ApiCommand<Tag>
    {
        public DeleteTag(Guid tagId)
        {
            TagId = tagId;
        }

        public Guid TagId { get; set; }
    }
}