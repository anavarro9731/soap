namespace Soap.Endpoint.Http.Infrastructure
{
    using System;
    using System.Reflection;
    using DataStore.Interfaces;
    using Soap.Interfaces;

    public static class HttpEndpoint
    {
        public static HttpEndpointConfiguration<TUserAuthenticator> Configure<TUserAuthenticator>(
            Assembly domainLogicAssembly,
            Assembly domainModelsAssembly,
            Func<IBusContext> busContextBuilder,
            Func<IDocumentRepository> documentRepositoryBuilder) where TUserAuthenticator : IAuthenticateUsers
        {
            return new HttpEndpointConfiguration<TUserAuthenticator>(domainLogicAssembly, domainModelsAssembly, busContextBuilder, documentRepositoryBuilder);
        }
    }
}
