namespace Soap.PfBase.Logic.ProcessesAndOperations
{
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Serilog;
    using Soap.Context.Context;
    using Soap.Interfaces;
    using Soap.Utility.Functions.Extensions;

    public class Operations<T> : IOperation where T : class, IAggregate, new()
    {
        private readonly ContextWithMessageLogEntry context = ContextWithMessageLogEntry.Current;

        protected T GetConfig<T>() where T: class, IBootstrapVariables => this.context.AppConfig.As<T>();
        
        public DataStoreReadOnly DataReader => this.context.DataStore.AsReadOnly();

        public DataStoreWriteOnly<T> DataWriter => this.context.DataStore.AsWriteOnlyScoped<T>();

        public IWithoutEventReplay DirectDataReader => this.context.DataStore.WithoutEventReplay;

        public ILogger Logger => this.context.Logger;
    }
}
