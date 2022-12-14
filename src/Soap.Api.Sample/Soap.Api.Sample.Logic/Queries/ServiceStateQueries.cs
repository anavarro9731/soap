namespace Soap.Api.Sample.Logic.Queries
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context;

    public class ServiceStateQueries : Query
    {
        public Func<Task<ServiceState>> GetServiceStateById =>
            async () => await DataReader.ReadById<ServiceState>(ServiceStateOperations.ServiceStateId);
    }
}
