namespace Soap.Api.Sso.Domain.Messages.Commands
{
    using System;
    using Soap.If.Interfaces.Messages;

    public class EnableUser : ApiCommand
    {
        public EnableUser(Guid idOfUserToEnable)
        {
            IdOfUserToEnable = idOfUserToEnable;
        }

        public Guid IdOfUserToEnable { get; }

        public override void Validate()
        {
            
        }
    }
}