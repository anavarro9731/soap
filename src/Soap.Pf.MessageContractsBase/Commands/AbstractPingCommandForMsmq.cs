namespace Soap.Pf.MessageContractsBase.Commands
{
    using System;
    using FluentValidation;
    using Soap.If.Interfaces.Messages;

    public abstract class AbstractPingCommandForMsmq<TResponse> : ApiCommand<TResponse> where TResponse : class, IApiCommand, new()
    {
        protected AbstractPingCommandForMsmq()
        {
        }

        protected AbstractPingCommandForMsmq(string pingedBy)
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

        public class Validator<TPongResponse> : AbstractValidator<AbstractPingCommandForMsmq<TPongResponse>>
            where TPongResponse : class, IApiCommand, new()
        {
            public Validator()
            {
                RuleFor(x => x.PingedBy).NotEmpty();
            }
        }
    }


}