﻿ //namespace Soap.NotificationServer.Channels
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Threading.Tasks;
//    using CircuitBoard.MessageAggregator;
//    using CircuitBoard.Messages;
//    using Mailer.NET.Mailer;
//    using Mailer.NET.Mailer.Transport;
//    using Soap.Utility.Functions.Operations;

//    public class EmailChannel : IServerChannelInfo
//    {
//        private readonly MailGunEmailSenderSettings emailSettings;

//        private readonly MailgunTransport mailGunClient;

//        public EmailChannel(MailGunEmailSenderSettings settings)
//        {
//            this.mailGunClient = new MailgunTransport(settings.MailGunDomain, settings.ApiKey);
//            this.emailSettings = settings;
//        }

//        public NotificationChannelTypes Type { get; } = NotificationChannelTypes.Email;

//        public Task Send(Notification notification)
//        {
//            var recipients = notification.Recipient.Split(';');
//            foreach (var s in recipients)
//                Guard.Against(
//                    () => !this.emailSettings.AllowedTo.Contains("*") && !this.emailSettings.AllowedTo.Contains(s),
//                    $"Sending emails to {s} is not allowed.");

//            return this.emailSettings.MessageAggregator
//                       .CollectAndForward(new SendingEmail(notification.Body, notification.Subject, recipients))
//                       .To(SendViaMailGun);
//        }

//        private Task SendViaMailGun(SendingEmail sendingEmail)
//        {
//            var emailMessage = new Email(this.mailGunClient)
//            {
//                Subject = sendingEmail.Subject,
//                Message = sendingEmail.Text,
//                From = new Contact
//                {
//                    Email = this.emailSettings.From
//                },
//                To = sendingEmail.SendTo.Select(
//                                     x => new Contact
//                                     {
//                                         Email = x
//                                     })
//                                 .ToList()
//            };
//            var result = this.mailGunClient.SendEmail(emailMessage);
//            Guard.Against(result.Success == false, "Could not send email: " + result.Message);
//            return Task.CompletedTask;
//        }

//        public class MailGunEmailSenderSettings : INotificationChannelSettings
//        {
//            internal readonly IMessageAggregator MessageAggregator;

//            public MailGunEmailSenderSettings(
//                string from,
//                string apiKey,
//                string mailGunDomain,
//                IReadOnlyList<string> allowedTo,
//                IMessageAggregator messageAggregator)
//            {
//                this.MessageAggregator = messageAggregator;
//                Guard.Against(string.IsNullOrEmpty(from), $"{nameof(MailGunEmailSenderSettings)}.{nameof(from)} cannot be null");
//                Guard.Against(
//                    string.IsNullOrEmpty(apiKey),
//                    $"{nameof(MailGunEmailSenderSettings)}.{nameof(apiKey)} cannot be null");
//                Guard.Against(
//                    string.IsNullOrEmpty(mailGunDomain),
//                    $"{nameof(MailGunEmailSenderSettings)}.{nameof(mailGunDomain)} cannot be null");

//                From = from;
//                ApiKey = apiKey;
//                MailGunDomain = mailGunDomain;
//                AllowedTo = allowedTo ?? new[]
//                {
//                    "*"
//                };
//            }

//            //the purpose of this is to prevent sending to bogus addresses when testing which causes problems for our mailgun account
//            public IReadOnlyList<string> AllowedTo { get; }

//            public string ApiKey { get; }

//            public string From { get; }

//            public string MailGunDomain { get; }

//            public IServerChannelInfo CreateChannel() => new EmailChannel(this);
//        }

//        public class SendingEmail : IChangeState
//        {
//            public SendingEmail(string text, string subject, string[] sendTo)
//            {
//                Text = text;
//                Subject = subject;
//                SendTo = sendTo;
//            }

//            public string[] SendTo { get; }

//            public double StateOperationCost { get; set; }

//            public TimeSpan? StateOperationDuration { get; set; }

//            public long StateOperationStartTimestamp { get; set; }

//            public long? StateOperationStopTimestamp { get; set; }

//            public string Subject { get; }

//            public string Text { get; }
//        }
//    }
//} TODO
