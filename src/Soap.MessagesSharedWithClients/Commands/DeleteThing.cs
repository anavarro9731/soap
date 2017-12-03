namespace Soap.Pf.ClientServerMessaging.Commands
{
    using System;
    using FluentValidation;
    using Soap.If.Interfaces.Messages;

    public class DeleteThing : ApiCommand
    {
        public DeleteThing(Guid thingId)
        {
            ThingId = thingId;
        }

        public Guid ThingId { get; set; }
    }

    public class DeleteThingValidator : AbstractValidator<DeleteThing>
    {
        public DeleteThingValidator()
        {
            RuleFor(x => x.ThingId).NotEmpty();
        }
    }
}