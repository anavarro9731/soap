namespace Soap.If.MessagePipeline
{
    using System;
    using System.IO;
    using CircuitBoard.MessageAggregator;
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Formatting.Json;
    using Soap.If.MessagePipeline.Messages;

    public class SerilogMessageAggregatorSink : ILogEventSink
    {
        private readonly IMessageAggregator messageAggregator;

        public SerilogMessageAggregatorSink(IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            using (var output = new StringWriter())
            {
                new JsonFormatter().Format(logEvent, output);

                this.messageAggregator.Collect(
                    SerilogEntryWithMeta.Create(
                        new SerilogEntry
                        {
                            Text = output.ToString()
                        }));
            }
        }
    }
}