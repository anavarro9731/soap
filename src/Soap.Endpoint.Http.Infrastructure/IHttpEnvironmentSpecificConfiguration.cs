namespace Soap.Endpoint.Http.Infrastructure
{
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Soap.Endpoint.Infrastructure;

    public interface IHttpEnvironmentSpecificConfiguration : IEnvironmentSpecificConfig
    {
        void DefineCorsPolicyPerEnvironment(CorsPolicyBuilder policyBuilder);
    }
}