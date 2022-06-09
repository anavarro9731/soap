namespace Soap.Context.Context
{
    using System.Threading;
    using System.Threading.Tasks;
    using Soap.Context.Logging;
    using Soap.Context.UnitOfWork;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class ContextWithMessageLogEntry : BoostrappedContext, IMessageFunctionsServerSide
    {
        public static readonly AsyncLocal<ContextWithMessageLogEntry> Instance = new AsyncLocal<ContextWithMessageLogEntry>();

        private readonly IMessageFunctionsServerSide functions;

        public ContextWithMessageLogEntry(MessageLogEntry messageLogEntry, ApiMessage message, BoostrappedContext current, UnitOfWork unitOfWork)
            : base(current)
        {
            MessageLogEntry = messageLogEntry;
            Message = message;
            UnitOfWork = unitOfWork;
            this.functions = this.MessageMapper.MapMessage(message);
        }

        public static ContextWithMessageLogEntry Current => Instance.Value;

        public ApiMessage Message { get; }
        
        public MessageLogEntry MessageLogEntry { get; }

        public UnitOfWork UnitOfWork { get; }

        public Task Handle(ApiMessage msg)
        {
            return this.functions.Handle(msg);
        }

        public Task HandleFinalFailure(MessageFailedAllRetries msg)
        {
            return this.functions.HandleFinalFailure(msg);
        }

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