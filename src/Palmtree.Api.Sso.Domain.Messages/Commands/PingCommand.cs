namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using System;
    using FluentValidation;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using Soap.If.Interfaces.Messages;

    public class PingCommand : ApiCommand<PongViewModel>
    {
        public PingCommand() { }

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