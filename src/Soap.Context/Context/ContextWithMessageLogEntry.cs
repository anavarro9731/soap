namespace Soap.Context.Context
{
    using System.Threading;
    using Soap.Context.Logging;

    public class ContextWithMessageLogEntry : ContextWithMessage
    {
        public static readonly AsyncLocal<ContextWithMessageLogEntry> Instance  = new AsyncLocal<ContextWithMessageLogEntry>();
        
        public ContextWithMessageLogEntry(MessageLogEntry messageLogEntry, ContextWithMessage current)
            : base(current)
        {
            MessageLogEntry = messageLogEntry;
        }

        public static ContextWithMessageLogEntry Current => Instance.Value;

        public MessageLogEntry MessageLogEntry { get; }
        
    }
    
    
}