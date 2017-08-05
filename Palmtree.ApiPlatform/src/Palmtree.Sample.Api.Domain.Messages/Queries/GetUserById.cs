namespace Palmtree.Sample.Api.Domain.Messages.Queries
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class GetUserById : ApiQuery
    {
        public GetUserById(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}
