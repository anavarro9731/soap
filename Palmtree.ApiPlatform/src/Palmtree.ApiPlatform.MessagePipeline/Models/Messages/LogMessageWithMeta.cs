namespace Palmtree.ApiPlatform.MessagePipeline.Models.Messages
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.IntraService;

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
