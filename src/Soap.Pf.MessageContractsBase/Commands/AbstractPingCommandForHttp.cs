namespace Soap.Pf.MessageContractsBase.Commands
{
    using System;
    using FluentValidation;
    using Soap.If.Interfaces.Messages;

    public abstract class AbstractPingCommandForHttp<TResponse> : ApiCommand<TResponse>
        where TResponse : AbstractPingCommandForHttp<TResponse>.AbstractResponseModel, new()
    {
        protected AbstractPingCommandForHttp()
        {
        }

        protected AbstractPingCommandForHttp(string pingedBy)
        {
            PingedBy = pingedBy;
        }

        public DateTime PingedAt { get; set; }

        public string PingedBy { get; set; }

        public override sealed void Validate()
        {
            new Validator<TResponse>().ValidateAndThrow(this);
        }

        public abstract class AbstractResponseModel
        {
            public DateTime PingedAt { get; set; }

            public string PingedBy { get; set; }

            public DateTime PongedAt { get; set; } = DateTime.Now;
        }

        public class Validator<TPongResponse> : AbstractValidator<AbstractPingCommandForHttp<TPongResponse>>
            where TPongResponse : AbstractPingCommandForHttp<TPongResponse>.AbstractResponseModel, new()
        {
            public Validator()
            {
                RuleFor(x => x.PingedBy).NotEmpty();
            }
        }
    }
}