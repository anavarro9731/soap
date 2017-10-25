namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using System;
    using Soap.Interfaces.Messages;

    public class EnableUser : ApiCommand
    {
        public EnableUser(Guid idOfUserToEnable)
        {
            IdOfUserToEnable = idOfUserToEnable;
        }

        public Guid IdOfUserToEnable { get; }
    }
}