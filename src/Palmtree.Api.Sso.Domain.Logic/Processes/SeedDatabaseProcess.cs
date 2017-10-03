namespace Palmtree.Api.Sso.Domain.Logic.Processes
{
    using System.Threading.Tasks;
    using FluentValidation;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Soap.Endpoint.Infrastructure;
    using Soap.MessagePipeline.Models;
    using Soap.MessagePipeline.ProcessesAndOperations;

    public class SeedDatabaseProcess : Process<SeedDatabaseProcess>, IBeginProcess<SeedDatabase>
    {
        private readonly UserOperations userOperations;

        public SeedDatabaseProcess(UserOperations userOperations)
        {
            this.userOperations = userOperations;
        }

        public async Task BeginProcess(SeedDatabase message, ApiMessageMeta meta)
        {
            {
                Validate();

                var defaultUser = await FindDefaultUser();

                if (defaultUser == null) await AddDefaultUser();
            }

            void Validate()
            {
                new SeedDatabaseValidator().ValidateAndThrow(message);
            }

            async Task<User> FindDefaultUser()
            {
                var userId = HardCodedMasterData.RootUser.UserId;
                var user = await DataStoreReadOnly.ReadActiveById<User>(userId);
                return user;
            }

            async Task AddDefaultUser()
            {
                await this.userOperations.AddDefaultUser();
            }
        }
    }
}
