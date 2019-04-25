namespace Soap.Api.Sso.Endpoint.Http.Handlers.Queries
{
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Messages.Queries;
    using Soap.Api.Sso.Domain.Models.Aggregates;
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