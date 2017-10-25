namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using System;
    using Soap.Interfaces.Messages;

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