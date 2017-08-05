namespace Palmtree.ApiPlatform.Endpoint.Http.Infrastructure
{
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Palmtree.ApiPlatform.Endpoint.Infrastructure;

    public interface IHttpEnvironmentSpecificConfiguration : IEnvironmentSpecificConfig
    {
        void DefineCorsPolicyPerEnvironment(CorsPolicyBuilder policyBuilder);
    }
}
