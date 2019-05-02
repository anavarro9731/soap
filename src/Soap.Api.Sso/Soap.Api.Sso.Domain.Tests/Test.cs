namespace Soap.Api.Sso.Domain.Tests
{
    using System.Linq;
    using Soap.Api.Sso.Domain.Logic.Configuration;
    using Soap.Api.Sso.Domain.Logic.Operations;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.Api.Sso.Domain.Models.ValueObjects;
    using Soap.Api.Sso.Endpoint.Http;
    using Soap.Api.Sso.Endpoint.Msmq;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Models;
    using Soap.Integrations.MailGun;
    using Soap.Pf.DomainLogicBase;
    using Soap.Pf.DomainTestsBase;

    public partial class Test
    {
        public readonly TestEndpoint endPoint;

        public Test()
        {
            this.endPoint = TestEndpoint.Configure<UserAuthenticator>(
                                            typeof(ServiceStateOperations).Assembly,
                                            typeof(UpgradeTheDatabaseCommand).Assembly,
                                            typeof(SoapApiSsoEndpointMsmq).Assembly,
                                            typeof(SoapApiSsoEndpointHttp).Assembly,
                                            new ApplicationConfiguration(
                                                "test",
                                                "0.0.0",
                                                new ApiEndpointSettings("httpAddress", "msmqAddress"),
                                                null,
                                                new MailGunEmailSenderSettings("from", "key", "to", null),
                                                true,
                                                3))
                                        .Start();
        }
    }

    public static class TestEndpointExtensions
    {
        public static string GetIdentityToken(this User user)
        {
            return SecurityToken.EncryptToken(user.ActiveSecurityTokens.First());
        }

        public static T HandleCommand<T>(this TestEndpoint testEndpoint, ApiCommand<T> command, User user) where T : class, new()
        {
            SetMessageIdentityToken(command, user);

            return testEndpoint.HandleCommand(command);
        }

        public static void HandleCommand(this TestEndpoint testEndpoint, ApiCommand command, User user)
        {
            SetMessageIdentityToken(command, user);

            testEndpoint.HandleCommand(command);
        }

        public static T HandleQuery<T>(this TestEndpoint testEndpoint, ApiQuery<T> query, User user) where T : class, new()
        {
            SetMessageIdentityToken(query, user);

            return testEndpoint.HandleQuery(query);
        }

        private static void SetMessageIdentityToken(IApiMessage message, User user)
        {
            message.IdentityToken = user?.GetIdentityToken();
        }
    }
}