namespace Palmtree.ApiPlatform.Infrastructure.Messages.Generic
{
    using DataStore.Interfaces.LowLevel;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class UpdateAggregate<T> : ApiCommand where T : IAggregate
    {
        public T UpdatedModel { get; set; }

        public static UpdateAggregate<T> Create(T updatedModel)
        {
            return new UpdateAggregate<T>
            {
                UpdatedModel = updatedModel
            };
        }
    }
}
