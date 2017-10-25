namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using System;
    using Soap.Interfaces.Messages;

    public class DeleteThing : ApiCommand
    {
        public DeleteThing(Guid thingId)
        {
            ThingId = thingId;
        }

        public Guid ThingId { get; set; }
    }
}