namespace Palmtree.ApiPlatform.Infrastructure.Messages.Generic
{
    using System;
    using DataStore.Interfaces.LowLevel;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class DeleteAggregate<T> : ApiCommand where T : IAggregate
    {
        public Guid IdToDelete { get; set; }

        public static DeleteAggregate<T> Create(Guid idToDelete)
        {
            return new DeleteAggregate<T>
            {
                IdToDelete = idToDelete
            };
        }
    }
}
