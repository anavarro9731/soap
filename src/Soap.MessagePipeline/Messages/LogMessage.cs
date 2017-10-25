namespace Soap.MessagePipeline.Messages
{
    using CircuitBoard.Messages;

    public class LogMessage : ILogMessage
    {
        public string Text { get; set; }

        public static LogMessage Create(string text)
        {
            return new LogMessage
            {
                Text = text
            };
        }
    }
}