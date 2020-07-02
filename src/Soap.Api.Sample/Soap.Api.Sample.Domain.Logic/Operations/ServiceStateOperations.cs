namespace Sample.Logic.Operations
{
    using System;
    using System.Threading.Tasks;
    using Sample.Models.Aggregates;
    using Sample.Models.Constants;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.Utility.Objects.Binary;

    public class ServiceStateOperations : Operations<ServiceState>
    {
        public static readonly Guid ServiceStateId = Guid.Parse("13de6317-9797-4cee-a69e-ecea7c5c8a5a");

        public Func<ReleaseVersions, Task<ServiceState>> SetDatabaseVersion =>
            newState =>
                {
                {
                    DetermineChange(out var changeDbVersion);

                    return DataWriter.UpdateById(ServiceStateId, changeDbVersion);
                }

                void DetermineChange(out Action<ServiceState> changeDbVersion)
                {
                    changeDbVersion = s => s.DatabaseState.AddState(newState);
                }
                };

        public Task<ServiceState> CreateServiceState()
        {
            {
                DetermineChange(out var serviceState);

                var result =  DataWriter.Create(serviceState);

                return result;
            }

            void DetermineChange(out ServiceState serviceState)
            {
                serviceState = new ServiceState
                {
                    DatabaseState = new Flags(ReleaseVersions.V1), id = ServiceStateId
                };
            }
        }
    }
}