namespace Soap.Pf.MsmqEndpointBase
{
    using System;
    using System.Reflection;
    using Autofac;
    using DataStore.Interfaces;
    using Soap.If.Interfaces;

    public static class MsmqEndpoint
    {
        public static MsmqEndpointConfiguration<TUserAuthenticator> Configure<TUserAuthenticator>(
            Assembly domainLogicAssembly,
            Assembly domainModelsAssembly,
            Func<IContainer, IBusContext> addBusToContainerFunc,
            Func<IDocumentRepository> buildDocumentRepositoryFunc) where TUserAuthenticator : IAuthenticateUsers
        {
            return new MsmqEndpointConfiguration<TUserAuthenticator>(domainLogicAssembly, domainModelsAssembly, addBusToContainerFunc, buildDocumentRepositoryFunc);
        }
    }
}