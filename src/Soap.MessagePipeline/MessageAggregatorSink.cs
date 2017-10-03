namespace Soap.MessagePipeline
{
    using System;
    using System.IO;
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Formatting.Json;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;
    using Soap.MessagePipeline.Messages;

    public class MessageAggregatorSink : ILogEventSink
    {
        private readonly IMessageAggregator messageAggregator;

        public MessageAggregatorSink(IMessageAggregator messageAggregator)
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
                    LogMessageWithMeta.Create(
                        new LogMessage
                        {
                            Text = output.ToString()
                        }));
            }
        }
    }
}
