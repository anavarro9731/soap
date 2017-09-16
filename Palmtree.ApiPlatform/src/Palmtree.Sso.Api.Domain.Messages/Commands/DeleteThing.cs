namespace Palmtree.Sample.Api.Domain.Messages.Commands
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class DeleteThing : ApiCommand
    {
        public DeleteThing(Guid thingId)
        {
            ThingId = thingId;
        }

        public Guid ThingId { get; set; }
    }
}