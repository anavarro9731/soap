namespace Soap.Integrations.Mailgun
{
    using Soap.If.Interfaces.Messages;

    public class SendEmail : ApiCommand
    {
        public SendEmail(string text, string subject, params string[] sendTo)
        {
            Text = text;
            Subject = subject;
            SendTo = sendTo;
        }

        public string[] SendTo { get; }

        public string Subject { get; }

        public string Text { get; }
    }
}