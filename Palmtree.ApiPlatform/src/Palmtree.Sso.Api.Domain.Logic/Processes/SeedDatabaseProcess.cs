namespace Palmtree.Sample.Api.Domain.Logic.Processes
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentValidation;
    using Palmtree.ApiPlatform.Endpoint.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations;
    using Palmtree.Sample.Api.Domain.Logic.Operations;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;

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
