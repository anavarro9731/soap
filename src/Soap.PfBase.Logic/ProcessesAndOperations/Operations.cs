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

    public class Operations<TAggregate> : IOperation where TAggregate : class, IAggregate, new()
    {
        private readonly ContextWithMessageLogEntry context = ContextWithMessageLogEntry.Current;

        public DataStoreReadOnly DataReader => this.context.DataStore.AsReadOnly();

        public DataStoreWriteOnly<TAggregate> DataWriter => this.context.DataStore.AsWriteOnlyScoped<TAggregate>();

        public IWithoutEventReplay DirectDataReader => this.context.DataStore.WithoutEventReplay;

        public ILogger Logger => this.context.Logger;

        protected BlobStorageWrapper BlobOperations => new BlobStorageWrapper(this.context);

        protected TDerivedConfig GetConfig<TDerivedConfig>() where TDerivedConfig : class, IBootstrapVariables => this.context.AppConfig.As<TDerivedConfig>();

        protected class BlobStorageWrapper
        {
            private readonly ContextWithMessageLogEntry context;

            public BlobStorageWrapper(ContextWithMessageLogEntry context)
            {
                this.context = context;
            }

            public async Task<TBlobType> DownloadAs<TBlobType>(Guid id, string containerName = "content") where TBlobType : class, new()
            {
                var blob = await this.context.BlobStorage.GetBlob(id, containerName);
                try
                {
                    return blob.ToObject<TBlobType>(SerialiserIds.JsonDotNetDefault);
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Could not deserialise blob to type " + typeof(TBlobType).FullName, e);
                }
            }

            public Task<bool> Exists(Guid id, string containerName = "content") =>
                this.context.BlobStorage.Exists(id, containerName);

            public Task Upload<TBlobType>(TBlobType @object, Func<TBlobType, Guid> getId, SerialiserIds serialiserId, string containerName = "content") =>
                this.context.BlobStorage.SaveObjectAsBlob(@object, getId, SerialiserIds.JsonDotNetDefault, containerName);
        }
    }
}
