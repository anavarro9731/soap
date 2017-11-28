namespace Palmtree.Api.Sso.Domain.Tests
{
    using System.Collections.Generic;
    using System.Reflection;
    using DataStore.Impl.SqlServer;
    using Palmtree.Api.Sso.Domain.Logic;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Palmtree.Api.Sso.Endpoint.Http.Handlers.Queries;
    using Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands;
    using Soap.DomainTests.Infrastructure;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Models;
    using Soap.ThirdPartyClients.Mailgun;

    public static class TestEnvironment
    {
        public static TestEndpoint CreateEndpoint()
        {
            var domainLogicAssembly = typeof(ThingOperations).Assembly;
            var domainModelsAssembly = typeof(Thing).Assembly;
            var handlerAssemblies = new List<Assembly>
            {
                typeof(PingHandler).Assembly,
                typeof(GetMessageFailedAllRetriesLogItemHandler).Assembly
            };
            IApplicationConfig applicationConfig = ApplicationConfiguration.Create(
                "Testing",
                Assembly.GetExecutingAssembly().GetName().Version.ToString(3),
                returnExplicitErrorMessages: true,
                numberOfApiMessageRetries: 3,
                apiEndpointSettings: ApiEndpointSettings.Create("httpUrl", "msmqAddress"),
                sqlServerDbSettings: SqlServerDbSettings.Create("serverInstance", "database", "userId", "password", "tableName"),
                mailgunEmailSenderSettings: MailgunEmailSenderSettings.Create("im@mycomputer.com", "apiKey", "domain"));

            var testEndpoint = TestEndpoint.Configure<UserAuthenticator>(domainLogicAssembly, domainModelsAssembly, handlerAssemblies, applicationConfig).Start();
            return testEndpoint;
        }

        public static object HandleCommand(this TestEndpoint testEndpoint, IApiCommand command, User user)
        {
            SetMessageIdentityToken(command, user);

            return testEndpoint.HandleCommand(command);
        }

        public static object HandleQuery(this TestEndpoint testEndpoint, IApiQuery query, User user)
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