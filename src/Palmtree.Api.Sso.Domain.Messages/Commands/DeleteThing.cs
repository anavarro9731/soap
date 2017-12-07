namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using System;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Soap.If.Interfaces.Messages;

    public class DeleteThing : ApiCommand<Thing>
    {
        public DeleteThing(Guid thingId)
        {
            ThingId = thingId;
        }

        public Guid ThingId { get; set; }
    }
}