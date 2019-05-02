namespace Soap.Pf.MessageContractsBase.Commands
{
    using System;
    using FluentValidation;
    using Soap.If.Interfaces.Messages;

    public abstract class AbstractPingCommand<TResponse> : ApiCommand<TResponse> where TResponse : AbstractPingCommand<TResponse>.AbstractResponseModel, new()
    {
        protected AbstractPingCommand()
        {
        }

        protected AbstractPingCommand(string pingedBy)
        {
            PingedBy = pingedBy;
        }

        public DateTime PingedAt { get; set; }

        public string PingedBy { get; set; }

        public abstract class AbstractResponseModel
        {
            public DateTime PingedAt { get; set; }

            public string PingedBy { get; set; }

            public DateTime PongedAt { get; set; } = DateTime.Now;
        }

        public override sealed void Validate()
        {
            new Validator<TResponse>().ValidateAndThrow(this);
        }

        public class Validator<TPongResponse> : AbstractValidator<AbstractPingCommand<TPongResponse>>
            where TPongResponse : AbstractPingCommand<TPongResponse>.AbstractResponseModel, new()
        {
            public Validator()
            {
                RuleFor(x => x.PingedBy).NotEmpty();
            }
        }
    }


}