namespace Soap.Context.Logging
{
    using CircuitBoard.Messages;

    public class SerilogEntry : ILogMessage
    {
        public string Text { get; set; }

        public static SerilogEntry Create(string text) =>
            new SerilogEntry
            {
                Text = text
            };
    }
}