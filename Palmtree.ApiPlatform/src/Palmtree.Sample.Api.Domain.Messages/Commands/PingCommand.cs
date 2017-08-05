namespace Palmtree.Sample.Api.Domain.Messages.Commands
{
    using System;
    using FluentValidation;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class PingCommand : ApiCommand
    {
        public PingCommand(string pingedBy)
        {
            PingedBy = pingedBy;
        }

        public DateTime PingedAt { get; set; } = DateTime.UtcNow;

        public string PingedBy { get; set; }
    }

    public class PingCommandValidator : AbstractValidator<PingCommand>
    {
        public PingCommandValidator()
        {
            RuleFor(x => x.PingedBy).NotEmpty();
        }
    }
}
