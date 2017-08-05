namespace Palmtree.Sample.Api.Domain.Messages.Commands
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class EnableUser : ApiCommand
    {
        public EnableUser(Guid idOfUserToEnable)
        {
            IdOfUserToEnable = idOfUserToEnable;
        }

        public Guid IdOfUserToEnable { get; }
    }
}
