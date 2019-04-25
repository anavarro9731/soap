namespace Soap.Api.Sso.Domain.Messages.Commands
{
    using System;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.If.Interfaces.Messages;

    public class AddATag : ApiCommand<Tag>
    {
        public AddATag(string nameOfTag)
        {
            NameOfTag = nameOfTag;
        }

        public string NameOfTag { get; set; }

        public Guid TagId { get; set; }
    }
}