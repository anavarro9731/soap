namespace Palmtree.Sample.Api.Domain.Messages.Commands
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class UpdateNameOfThing : ApiCommand
    {
        public UpdateNameOfThing(Guid thingId, string nameOfThing)
        {
            ThingId = thingId;
            NameOfThing = nameOfThing;
        }

        public string NameOfThing { get; set; }

        public Guid ThingId { get; set; }
    }
}