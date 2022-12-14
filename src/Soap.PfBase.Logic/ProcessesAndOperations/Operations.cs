namespace Soap.PfBase.Logic.ProcessesAndOperations
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Serilog;
    using Soap.Context.BlobStorage;
    using Soap.Context.Context;
    using Soap.Interfaces;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;

    public class Operations<TAggregate> : IOperation where TAggregate : class, IAggregate, new()
    {
        private readonly ContextWithMessageLogEntry context = ContextWithMessageLogEntry.Current;

        protected IDataStoreReadOnly DataReader => this.context.DataStore.AsReadOnly();

        protected IDataStoreWriteOnly<TAggregate> DataWriter => this.context.DataStore.AsWriteOnlyScoped<TAggregate>();

        protected ILogger Logger => this.context.Logger;

        protected MessageMeta Meta => this.context.MessageLogEntry.MessageMeta;
        
        protected BlobStorageWrapper BlobOperations => new BlobStorageWrapper(this.context);

        protected T GetCustomConfigVariable<T>(string propertyName)
        {
            var propertyInfo = this.context.AppConfig.GetType().GetProperty(propertyName);
            Guard.Against(propertyInfo == null, $"Custom config property {propertyName} not found");
            return (T)propertyInfo.GetValue(this.context.AppConfig, null);
        }

        protected class BlobStorageWrapper
        {
            private readonly ContextWithMessageLogEntry context;

            public BlobStorageWrapper(ContextWithMessageLogEntry context)
            {
                this.context = context;
            }

            public async Task<TBlobType> DownloadAs<TBlobType>(Guid id, string containerName = "content", SerialiserIds serialiserId = null) where TBlobType : class, new()
            {
                var blob = await this.context.BlobStorage.GetBlobOrNull(id, containerName);
                if (blob == null) return null;
                try
                {
                    return blob.ToObject<TBlobType>(serialiserId ?? SerialiserIds.JsonDotNetDefault);
                }
                catch (Exception e)
                {
                    throw new CircuitException("Could not deserialise blob to type " + typeof(TBlobType).FullName, e);
                }
            }

            public Task<bool> Exists(Guid id, string containerName = "content") =>
                this.context.BlobStorage.Exists(id, containerName);

            public Task Upload<TBlobType>(TBlobType @object, Func<TBlobType, Guid> getId, string containerName = "content", SerialiserIds serialiserId = null) =>
                this.context.BlobStorage.SaveObjectAsBlob(@object, getId, serialiserId ?? SerialiserIds.JsonDotNetDefault, containerName);
        }
    }
}
