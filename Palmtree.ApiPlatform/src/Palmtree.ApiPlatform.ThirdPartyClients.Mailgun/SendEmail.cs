namespace Palmtree.ApiPlatform.ThirdPartyClients.Mailgun
{
    using Mailer.NET.Mailer;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class SendEmail : ApiCommand
    {
        public SendEmail(Email message)
        {
            Message = message;
        }

        public Email Message { get; }
    }
}

