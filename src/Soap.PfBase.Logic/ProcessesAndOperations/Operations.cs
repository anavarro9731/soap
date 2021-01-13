namespace Soap.PfBase.Logic.ProcessesAndOperations
{
    using System;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Serilog;
    using Soap.Context.BlobStorage;
    using Soap.Context.Context;
    using Soap.Interfaces;
    using Soap.Utility.Functions.Extensions;

    public class Operations<T> : IOperation where T : class, IAggregate, new()
    {
        private readonly ContextWithMessageLogEntry context = ContextWithMessageLogEntry.Current;

        public DataStoreReadOnly DataReader => this.context.DataStore.AsReadOnly();

        public DataStoreWriteOnly<T> DataWriter => this.context.DataStore.AsWriteOnlyScoped<T>();

        public IWithoutEventReplay DirectDataReader => this.context.DataStore.WithoutEventReplay;

        public ILogger Logger => this.context.Logger;

        protected BlobStorageWrapper BlobOperations => new BlobStorageWrapper(this.context);

        protected T GetConfig<T>() where T : class, IBootstrapVariables => this.context.AppConfig.As<T>();

        protected class BlobStorageWrapper
        {
            private readonly ContextWithMessageLogEntry context;

            public BlobStorageWrapper(ContextWithMessageLogEntry context)
            {
                this.context = context;
            }

            public async Task<T> DownloadAs<T>(Guid id, string containerName = "content") where T : class, new()
            {
                var blob = await this.context.BlobStorage.GetBlob(id, containerName);
                try
                {
                    return blob.ToObject<T>(SerialiserIds.JsonDotNetDefault);
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Could not deserialise blob to type " + typeof(T).FullName, e);
                }
            }

            public Task<bool> Exists(Guid id, string containerName = "content") =>
                this.context.BlobStorage.Exists(id, containerName);

            public Task Upload<T>(T @object, Func<T, Guid> getId, SerialiserIds serialiserId, string containerName = "content") =>
                this.context.BlobStorage.SaveObjectAsBlob(@object, getId, SerialiserIds.JsonDotNetDefault, containerName);
        }
    }
}
