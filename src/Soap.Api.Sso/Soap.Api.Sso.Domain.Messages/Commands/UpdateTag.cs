namespace Soap.Api.Sso.Domain.Messages.Commands
{
    using System;
    using Soap.If.Interfaces.Messages;

    public class UpdateNameOfTag : ApiCommand
    {
        public UpdateNameOfTag(Guid tagId, string nameOfTag)
        {
            TagId = tagId;
            NameOfTag = nameOfTag;
        }

        public string NameOfTag { get; set; }

        public Guid TagId { get; set; }

        public override void Validate()
        {
            
        }
    }
}