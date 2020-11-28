namespace Soap.Api.Sample.Logic.Operations
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Soap.Api.Sample.Constants;
    using Soap.Api.Sample.Logic.Queries;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Soap.Utility.Functions.Operations;
    using Soap.Utility.Objects.Binary;

    public class ServiceStateOperations : Operations<ServiceState>
    {

        public static readonly Guid ServiceStateId = Guid.Parse("13de6317-9797-4cee-a69e-ecea7c5c8a5a");

        public Func<Task<ServiceState>> CreateServiceState =>
            () =>
                {
                {
                    DetermineChange(out var serviceState);

                    var result = DataWriter.Create(serviceState);

                    return result;
                }

                void DetermineChange(out ServiceState serviceState)
                {
                    serviceState = new ServiceState
                    {
                        id = ServiceStateId,
                        DatabaseState = new EnumerationFlags(ReleaseVersions.V1)
                    };
                }
                };

        public Func<ReleaseVersions, Task<ServiceState>> SetDatabaseVersion =>
            async newState =>
                {
                {
                    await Validate();

                    DetermineChange(out var changeDbVersion);

                    return await DataWriter.UpdateById(ServiceStateId, changeDbVersion);
                }

                async Task Validate()
                {
                    var s = await this.Get<ServiceStateQueries>().Call(x => x.GetServiceState)();

                    Guard.Against(
                        s.DatabaseState.HasFlag(newState),
                        ErrorCodes.AttemptingToUpgradeDatabaseToOutdatedVersion);
                }

                void DetermineChange(out Action<ServiceState> changeDbVersion)
                {
                    changeDbVersion = s => s.DatabaseState.AddFlag(newState);
                }
                };

        public class ErrorCodes : ErrorCode
        {
            public static readonly ErrorCode AttemptingToUpgradeDatabaseToOutdatedVersion = Create(
                Guid.Parse("b866824e-ccc2-4f84-8399-15877bf735e9"),
                "Attempting To Upgrade Database To Outdated Version");
        }
    }
}
