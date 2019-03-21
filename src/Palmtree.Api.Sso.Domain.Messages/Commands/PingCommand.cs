namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using System;
    using FluentValidation;
    using Soap.If.Interfaces.Messages;

    public class PingCommand : ApiCommand<PingCommand.PongViewModel>
    {
        public PingCommand()
        {
        }

        public PingCommand(string pingedBy)
        {
            PingedBy = pingedBy;
        }

        public DateTime PingedAt { get; set; } = DateTime.UtcNow;

        public string PingedBy { get; set; }

        public class PongViewModel
        {
            public DateTime PingedAt { get; set; }

            public string PingedBy { get; set; }

            public DateTime PongedAt { get; set; } = DateTime.Now;
        }
    }

    public class PingCommandValidator : AbstractValidator<PingCommand>
    {
        public PingCommandValidator()
        {
            RuleFor(x => x.PingedBy).NotEmpty();
        }
    }
}