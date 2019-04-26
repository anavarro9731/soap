namespace Soap.Api.Sso.Domain.Messages.Commands
{
    using System;
    using Soap.If.Interfaces.Messages;

    public class DisableUser : ApiCommand
    {
        public DisableUser(Guid idOfUserToDisable)
        {
            IdOfUserToDisable = idOfUserToDisable;
        }

        public Guid IdOfUserToDisable { get; }
    }
}