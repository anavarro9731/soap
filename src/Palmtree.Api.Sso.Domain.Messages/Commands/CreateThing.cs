namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using System;
    using Soap.If.Interfaces.Messages;

    public class CreateThing : ApiCommand
    {
        public CreateThing(string nameOfThing)
        {
            NameOfThing = nameOfThing;
        }

        public string NameOfThing { get; set; }

        public Guid ThingId { get; set; }
    }
}