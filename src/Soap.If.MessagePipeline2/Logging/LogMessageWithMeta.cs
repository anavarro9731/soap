namespace Soap.If.MessagePipeline.Messages
{
    using System;
    using CircuitBoard.Messages;

    public class LogMessageWithMeta : ILogMessage
    {
        public DateTime Created { get; set; }

        public string Text { set => Message.Text = value; get => Message.Text; }

        private ILogMessage Message { get; set; }

        public static LogMessageWithMeta Create(ILogMessage logMessage)
        {
            return new LogMessageWithMeta
            {
                Message = logMessage,
                Created = DateTime.UtcNow
            };
        }
    }
}