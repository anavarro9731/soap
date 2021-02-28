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

        public void Validate(ApiMessage message)
        {
            if (message is MessageFailedAllRetries)
            {
                message.Validate();
                /* validate it directly rather than through associated messagefunctions object, because
                you cannot use the function.Validate() call because that is expecting the type of 
                the message that failed, and it will attempt a cast which will break. We use the functions for the
                message that failed and not MessageFailedAllRetries because that is where the HandleFinalFailure 
                function which is the actual handler for this message is located */
            }
            else
            {
                this.functions.Validate(message);    
            }
        }
    }
}
