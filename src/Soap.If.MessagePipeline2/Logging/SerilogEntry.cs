namespace Soap.If.MessagePipeline.Messages
{
    using CircuitBoard.Messages;

    public class SerilogEntry : ILogMessage
    {
        public string Text { get; set; }

        public static SerilogEntry Create(string text)
        {
            return new SerilogEntry
            {
                Text = text
            };
        }
    }
}