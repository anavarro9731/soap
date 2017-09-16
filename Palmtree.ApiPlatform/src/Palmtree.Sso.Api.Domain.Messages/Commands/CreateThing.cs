namespace Palmtree.Sample.Api.Domain.Messages.Commands
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class CreateThing : ApiCommand
    {
        public CreateThing(string nameOfThing)
        {
            NameOfThing = nameOfThing;
        }

        public Guid ThingId { get; set; }

        public string NameOfThing { get; set; }
    }
}