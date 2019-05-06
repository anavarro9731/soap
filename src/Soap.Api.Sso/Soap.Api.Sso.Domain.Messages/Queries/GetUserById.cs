﻿namespace Soap.Api.Sso.Domain.Messages.Queries
{
    using System;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.If.Interfaces.Messages;

    public class GetUserById : ApiQuery<User>
    {
        public GetUserById(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }

        public override void Validate()
        {
            
        }
    }
}