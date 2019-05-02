namespace Soap.Api.Sample.Domain.Tests
{
    using System.Linq;
    using Soap.Api.Sample.Domain.Logic.Configuration;
    using Soap.Api.Sample.Domain.Logic.Operations;
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.Api.Sample.Domain.Models;
    using Soap.Api.Sample.Domain.Models.ValueObjects;
    using Soap.Api.Sample.Domain.Models.ViewModels;
    using Soap.Api.Sample.Endpoint.Http;
    using Soap.Api.Sample.Endpoint.Msmq;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.DomainLogicBase;
    using Soap.Pf.DomainTestsBase;
    using Soap.Pf.EndpointInfrastructure;

    public partial class Test
    {
        public readonly TestEndpoint endPoint;

        public Test()
        {
            this.endPoint = TestEndpoint.Configure<UserAuthenticator>(
                                            typeof(ServiceStateOperations).Assembly,
                                            typeof(UpgradeTheDatabaseCommand).Assembly,
                                            typeof(SoapApiSampleEndpointMsmq).Assembly,
                                            typeof(SoapApiSampleEndpointHttp).Assembly,
                                            new ApplicationConfiguration("test", "0.0.0", new ApiEndpointSettings("httpAddress", "msmqAddress"), null, null, true))
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