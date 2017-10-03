namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class DisableUser : ApiCommand
    {
        public DisableUser(Guid idOfUserToDisable)
        {
            IdOfUserToDisable = idOfUserToDisable;
        }

        public Guid IdOfUserToDisable { get; }
    }
}
