namespace Soap.UnitTests
{
    using System;
    using FluentAssertions;
    using FluentValidation;
    using Soap.Client;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
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

        public class WhenWeSubmitACommandByHttp
        {
            private static readonly SoapClient client = new SoapClient();

            private static SoapClient.HttpSendResult result;

            public WhenWeSubmitACommandByHttp()
            {
                var x = new C100_TestCommand
                {
                    Name = "test"
                };

                result = client.Send(
                                   "http://localhost/",
                                   x,
                                   new SoapClient.HttpOptions()
                                   {
                                       TestMode = true,
                                       RequiresAuth = new SoapClient.OptionsBase.Auth("accesstoken", "idtoken", "user"),
                                       SignalRSession = new SoapClient.OptionsBase.SignalRSessionInfo("hash", Guid.NewGuid(), "sessionid")
                                   })
                               .Result.CastOrError<SoapClient.HttpSendResult>();
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
        
        public class WhenWeSubmitACommandByBus
        {
            private static readonly SoapClient client = new SoapClient();

            private static SoapClient.BusSendResult result;

            public WhenWeSubmitACommandByBus()
            {
                var x = new C100_TestCommand
                {
                    Name = "test"
                };

                result = client.Send(
                                   "Endpoint=sb://sb-soapapisample-vnext.servicebus.windows.net/;SharedAccessKeyName=SenderAccessKey;SharedAccessKey=u5GzCw0bot5/Xc5EIq4X6B3fD70vYE65Bso2AZ1vI+8=",
                                   x,
                                   new SoapClient.BusOptions()
                                   {
                                       ScheduleAt = new DateTimeOffset(DateTime.Now.AddDays(1)),
                                       BusSessionId = Guid.NewGuid(),
                                       TestMode = true,
                                       RequiresAuth = new SoapClient.OptionsBase.Auth("accesstoken", "idtoken", "user"),
                                       SignalRSession = new SoapClient.OptionsBase.SignalRSessionInfo("hash", Guid.NewGuid(), "sessionid")
                                   })
                               .Result.CastOrError<SoapClient.BusSendResult>();
            }

            [Fact]
            public void ItShouldSerialiseIt()
            {
                result.Should().NotBeNull();
                result.MessageId.Should().NotBeEmpty();
                result.JsonSent.Should().NotBeEmpty();
            }
        }
        
    }
}