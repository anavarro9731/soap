namespace Soap.Api.Sample.Domain.Logic.Operations
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Domain.Constants;
    using Soap.Api.Sample.Domain.Models.Aggregates;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.If.Utility;

    public class ServiceStateOperations : Operations<ServiceState>
    {
        public static readonly Guid ServiceStateId = Guid.Parse("13de6317-9797-4cee-a69e-ecea7c5c8a5a");

        public Task<ServiceState> CreateServiceState()
        {
            {
                DetermineChange(out var serviceState);

                return DataStore.Create(serviceState);
            }

            void DetermineChange(out ServiceState serviceState)
            {
                serviceState = new ServiceState
                {
                    DatabaseState = FlaggedState.Create(ReleaseVersions.v1), id = ServiceStateId
                };
            }
        }

        public Task<ServiceState> SetDatabaseVersion(ReleaseVersions newState)
        {
            {
                DetermineChange(out var changeDbVersion);

                return DataStore.UpdateById(ServiceStateId, changeDbVersion);
            }

            void DetermineChange(out Action<ServiceState> changeDbVersion)
            {
                changeDbVersion = s => s.DatabaseState.AddState(newState);
            }
        }
    }
}