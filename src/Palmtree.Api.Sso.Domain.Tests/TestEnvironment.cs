namespace Palmtree.Api.Sso.Domain.Tests
{
    using System.Reflection;
    using DataStore.Impl.SqlServer;
    using Palmtree.Api.Sso.Domain.Logic;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Palmtree.Api.Sso.Endpoint.Http;
    using Palmtree.Api.Sso.Endpoint.Msmq;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Models;
    using Soap.Integrations.Mailgun;
    using Soap.Pf.DomainTestsBase;

    public static class TestEnvironment
    {
        public static TestEndpoint CreateEndpoint()
        {
            var domainLogicAssembly = typeof(ThingOperations).Assembly;
            var domainMessagesAssembly = typeof(SeedDatabase).Assembly;

            IApplicationConfig applicationConfig = ApplicationConfiguration.Create(
                "Testing",
                Assembly.GetExecutingAssembly().GetName().Version.ToString(3),
                returnExplicitErrorMessages: true,
                numberOfApiMessageRetries: 3,
                apiEndpointSettings: ApiEndpointSettings.Create("httpUrl", "msmqAddress"),
                sqlServerDbSettings: SqlServerDbSettings.Create("serverInstance", "database", "userId", "password", "tableName"),
                mailgunEmailSenderSettings: MailgunEmailSenderSettings.Create("im@mycomputer.com", "apiKey", "domain"));

            var testEndpoint = TestEndpoint.Configure<UserAuthenticator>(
                                               domainLogicAssembly,
                                               domainMessagesAssembly,
                                               PalmTreeApiSsoEndpointMsmq.GetAssembly,
                                               PalmTreeApiSsoEndpointHttp.GetAssembly,
                                               applicationConfig)
                                           .Start();
            return testEndpoint;
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