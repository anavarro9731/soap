namespace Palmtree.Api.Sso.Domain.Messages.Queries
{
    using System;
    using Soap.If.Interfaces.Messages;

    public class GetUserById : ApiQuery
    {
        public GetUserById(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}