namespace Palmtree.ApiPlatform.Infrastructure.Messages.Generic
{
    using DataStore.Interfaces.LowLevel;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class CreateAggregate<T> : ApiCommand where T : IAggregate
    {
        public T Model { get; set; }

        public static CreateAggregate<T> Create(T model)
        {
            return new CreateAggregate<T>
            {
                Model = model
            };
        }
    }
}
