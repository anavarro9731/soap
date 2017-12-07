namespace Palmtree.Api.Sso.Endpoint.Http.Handlers.Queries
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Messages.Queries;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.HttpEndpointBase;

    public class GetUserByIdHandler : QueryHandler<GetUserById, User>
    {
        protected override async Task<User> Handle(GetUserById message, ApiMessageMeta meta)
        {
            return await DataStore.ReadActiveById<User>(message.Id);
        }
    }
}