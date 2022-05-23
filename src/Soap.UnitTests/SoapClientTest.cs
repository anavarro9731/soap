namespace Soap.UnitTests
{
    using System;
    using FluentAssertions;
    using FluentValidation;
    using Soap.Client;
    using Soap.Interfaces.Messages;
    using Xunit;

    public class SoapClientTest
    {
        public class C100_TestCommand : ApiCommand
        {
            public string Name { get; set; }

            public override void Validate()
            {
                new Validator().ValidateAndThrow(this);
            }

            public class Validator : AbstractValidator<C100_TestCommand>
            {
                public Validator()
                {
                    RuleFor(x => x.Name).NotEmpty();
                }
            }
        }

        public class WhenWeSubmitACommand
        {
            private static readonly SoapClient client = new SoapClient();

            private static SoapClient.SendResult result;

            public WhenWeSubmitACommand()
            {
                var x = new C100_TestCommand
                {
                    Name = "test"
                };

                result = client.Send(
                                   "http://localhost/",
                                   x,
                                   new SoapClient.Options
                                   {
                                       TestMode = true,
                                       RequiresAuth = new SoapClient.Options.Auth
                                       {
                                           Username = "user",
                                           AccessToken = "accesstoken",
                                           IdentityToken = "idtoken"
                                       },
                                       SignalRSession = new SoapClient.Options.SignalRSessionInfo
                                       {
                                           CommandHash = "hash",
                                           ConversationId = Guid.NewGuid(),
                                           SignalRSessionId = "sessionid"
                                       }
                                   })
                               .Result;
            }

            [Fact]
            public void ItShouldSerialiseIt()
            {
                result.HttpResponseMessage.RequestMessage.RequestUri.Should()
                      .Be(
                          $"http://localhost/api/ReceiveMessageHttp?id={result.MessageId}&type=Soap.UnitTests.SoapClientTest%2BC100_TestCommand%2C%20Soap.UnitTests");
                result.Should().NotBeNull();
                result.MessageId.Should().NotBeEmpty();
                result.JsonSent.Should().NotBeEmpty();
            }
        }
    }
}