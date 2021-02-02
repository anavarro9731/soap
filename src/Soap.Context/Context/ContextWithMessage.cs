namespace Soap.Context.Context
{
    using System;
    using System.Threading.Tasks;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class ContextWithMessage : BoostrappedContext, IMessageFunctionsServerSide
    {
        private readonly IMessageFunctionsServerSide functions;

        public ContextWithMessage(
            ApiMessage message,
            BoostrappedContext context)
            : base(context)
        {
            Message = message;
            this.functions = this.MessageMapper.MapMessage(message);
        }

        protected ContextWithMessage(ContextWithMessage c)
            : base(c)
        {
            Message = c.Message;
            this.functions = c.functions;
        }

        public ApiMessage Message { get; }

        public Task Handle(ApiMessage msg) => this.functions.Handle(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) => this.functions.HandleFinalFailure(msg);

        public void Validate(ApiMessage msg) => this.functions.Validate(msg);
    }
}
