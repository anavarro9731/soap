namespace Sample.Logic.Queries
{
    using System;
    using System.Threading.Tasks;
    using Sample.Logic.Operations;
    using Sample.Models.Aggregates;
    using Soap.MessagePipeline;

    public class ServiceStateQueries : Query
    {
        public Func<Task<ServiceState>> GetServiceState =>
            async () => await DataReader.ReadById<ServiceState>(ServiceStateOperations.ServiceStateId);
    }
}