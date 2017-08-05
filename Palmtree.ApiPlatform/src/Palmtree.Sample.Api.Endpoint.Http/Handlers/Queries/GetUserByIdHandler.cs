namespace Palmtree.Sample.Api.Endpoint.Http.Handlers.Queries
{
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.Sample.Api.Domain.Messages.Queries;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;

    public class GetUserByIdHandler : MessageHandler<GetUserById, User>
    {
        protected override async Task<User> Handle(GetUserById message, ApiMessageMeta meta)
        {
            return await DataStore.ReadActiveById<User>(message.Id);
        }
    }
}
