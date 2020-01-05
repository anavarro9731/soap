namespace Soap.If.MessagePipeline.Messages
{
    using System;
    using CircuitBoard.Messages;

    public class SerilogEntryWithMeta : ILogMessage
    {
        public DateTime Created { get; set; }

        public string Text { set => Message.Text = value; get => Message.Text; }

        private ILogMessage Message { get; set; }

        public static SerilogEntryWithMeta Create(ILogMessage logMessage)
        {
            return new SerilogEntryWithMeta
            {
                Message = logMessage,
                Created = DateTime.UtcNow
            };
        }
    }
}