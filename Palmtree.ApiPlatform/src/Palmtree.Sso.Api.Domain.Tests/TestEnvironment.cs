namespace Palmtree.Sample.Api.Domain.Tests
{
    using System.Collections.Generic;
    using System.Reflection;
    using DataStore.Impl.DocumentDb.Config;
    using DataStore.Impl.SqlServer;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Palmtree.ApiPlatform.DomainTests.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Interfaces;
    using Palmtree.ApiPlatform.ThirdPartyClients.Mailgun;
    using Palmtree.Sample.Api.Domain.Logic;
    using Palmtree.Sample.Api.Domain.Logic.Operations;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;
    using Palmtree.Sample.Api.Endpoint.Http.Handlers.Queries;
    using Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands;

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
                returnExplicitErrorMessages: true,
                numberOfApiMessageRetries: 3,
                apiServerSettings: ApiServerSettings.Create("httpUrl", "msmqAddress"),
                sqlServerDbSettings: SqlServerDbSettings.Create("serverInstance", "database", "userId", "password", "tableName"),
                fileStorageSettings: FileStorageSettings.Create("connString", "storagePrefix"),
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
