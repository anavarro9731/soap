namespace Soap.Context.Context
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Soap.Context.Logging;
    using Soap.Interfaces;

    public class ContextWithMessageLogEntry : ContextWithMessage
    {
        public static readonly AsyncLocal<ContextWithMessageLogEntry> Instance = new AsyncLocal<ContextWithMessageLogEntry>();
        
        public ContextWithMessageLogEntry(
            MessageLogEntry messageLogEntry,
            ContextWithMessage current)
            : base(current)
        {
            MessageLogEntry = messageLogEntry;
        }

        public static ContextWithMessageLogEntry Current => Instance.Value;
        
        public MessageLogEntry MessageLogEntry { get; }
    }
}
