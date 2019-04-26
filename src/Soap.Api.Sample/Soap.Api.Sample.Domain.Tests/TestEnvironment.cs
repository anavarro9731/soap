namespace Soap.Api.Sample.Domain.Tests
{
    using System.Linq;
    using System.Reflection;
    using CircuitBoard.Permissions;
    using DataStore.Providers.CosmosDb;
    using Soap.Api.Sample.Domain.Logic.Configuration;
    using Soap.Api.Sample.Domain.Logic.Operations;
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.Api.Sample.Domain.Models;
    using Soap.Api.Sample.Domain.Models.Aggregates;
    using Soap.Api.Sample.Domain.Models.ValueObjects;
    using Soap.Api.Sample.Endpoint.Http;
    using Soap.Api.Sample.Endpoint.Msmq;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Models;
    using Soap.Integrations.Mailgun;
    using Soap.Pf.DomainTestsBase;

    public static class TestEnvironment
    {
        public static TestEndpoint CreateEndpoint()
        {
            var domainLogicAssembly = typeof(ServiceStateOperations).Assembly;
            var domainMessagesAssembly = typeof(UpgradeTheDatabase).Assembly;

            IApplicationConfig applicationConfig = ApplicationConfiguration.Create(
                "Testing",
                Assembly.GetExecutingAssembly().GetName().Version.ToString(3),
                returnExplicitErrorMessages: true,
                numberOfApiMessageRetries: 3,
                apiEndpointSettings: ApiEndpointSettings.Create("httpUrl", "msmqAddress"),
                cosmosStoreSettings: new CosmosSettings("testdb", "https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="),
                mailgunEmailSenderSettings: MailgunEmailSenderSettings.Create("im@mycomputer.com", "apiKey", "domain"));

            var testEndpoint = TestEndpoint.Configure<UserAuthenticator>(
                                               domainLogicAssembly,
                                               domainMessagesAssembly,
                                               SoapApiSampleEndpointMsmq.GetAssembly,
                                               SoapApiSampleEndpointHttp.GetAssembly,
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

        public static string GetIdentityToken(this User user)
        {
            return SecurityToken.EncryptToken(user.ActiveSecurityTokens.First());
        }
    }
}