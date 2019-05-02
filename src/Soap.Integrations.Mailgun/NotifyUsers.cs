namespace Soap.Integrations.MailGun
{
    using Soap.If.Interfaces.Messages;

    public class NotifyUsers : ApiCommand
    {
        public NotifyUsers(string text, string subject, params string[] sendTo)
        {
            Text = text;
            Subject = subject;
            SendTo = sendTo;
        }

        public string[] SendTo { get; }

        public string Subject { get; }

        public string Text { get; }

        public override void Validate()
        {
            
        }
    }
}