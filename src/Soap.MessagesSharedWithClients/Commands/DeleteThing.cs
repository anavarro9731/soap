namespace Soap.MessagesSharedWithClients.Commands
{
    using System;
    using FluentValidation;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class DeleteThing : ApiCommand
    {
        public Guid ThingId { get; set; }

        public DeleteThing(Guid thingId)
        {
            ThingId = thingId;
        }
    }

    public class DeleteThingValidator : AbstractValidator<DeleteThing>
    {
        public DeleteThingValidator()
        {
            RuleFor(x => x.ThingId).NotEmpty();
        }
    }
}