namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using System;
    using Soap.Interfaces.Messages;

    public class DisableUser : ApiCommand
    {
        public DisableUser(Guid idOfUserToDisable)
        {
            IdOfUserToDisable = idOfUserToDisable;
        }

        public Guid IdOfUserToDisable { get; }
    }
}