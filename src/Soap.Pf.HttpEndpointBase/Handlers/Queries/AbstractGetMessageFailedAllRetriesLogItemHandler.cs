namespace Soap.Pf.HttpEndpointBase.Handlers.Queries
{
    using System.Linq;
    using System.Threading.Tasks;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.Utility.PureFunctions.Extensions;
    using Soap.Pf.DomainModelsBase;
    using Soap.Pf.MessageContractsBase.Queries;

    public abstract class AbstractGetMessageFailedAllRetriesLogItemHandler<TQuery, TResponse> : QueryHandler<TQuery, TResponse>
        where TQuery : AbstractGetMessageFailedAllRetriesLogItem<TResponse>, new()
        where TResponse : AbstractGetMessageFailedAllRetriesLogItem<TResponse>.AbstractResponseModel, new()
    {
        protected override async Task<TResponse> Handle(TQuery message, ApiMessageMeta meta)
        {

            
            var dbModel = (await DataStore.ReadActive<MessageFailedAllRetriesLogItem>(m => m.IdOfMessageThatFailed == message.IdOfMessageYouWantResultsFor))
                .SingleOrDefault();

            return dbModel.Map(
                db => new TResponse
                {
                    IdOfMessageThatFailed = db.IdOfMessageThatFailed
                });
        }
    }
}