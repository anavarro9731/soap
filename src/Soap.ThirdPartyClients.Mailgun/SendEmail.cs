namespace Soap.Integrations.Mailgun
{
    using Mailer.NET.Mailer;
    using Soap.If.Interfaces.Messages;

    public class SendEmail : ApiCommand
    {
        public SendEmail(Email message)
        {
            Message = message;
        }

        public Email Message { get; }
    }
}