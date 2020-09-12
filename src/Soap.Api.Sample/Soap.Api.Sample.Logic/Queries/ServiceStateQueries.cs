namespace Soap.Api.Sample.Logic.Queries
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.MessagePipeline;

    public class ServiceStateQueries : Query
    {
        public Func<Task<ServiceState>> GetServiceState =>
            async () => await DataReader.ReadById<ServiceState>(ServiceStateOperations.ServiceStateId);
    }
}