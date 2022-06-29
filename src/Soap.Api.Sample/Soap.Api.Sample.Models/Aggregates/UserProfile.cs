namespace Soap.Api.Sample.Models.Aggregates
{
    using System;
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;
    using Soap.Interfaces;

    public class UserProfile : Aggregate, IUserProfile
    {
        [ScopeObjectReference(typeof(Tenant))]
        public Guid? TenantId { get; set; }
        
        public string IdaamProviderId { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }
        
        public string AnyString { get; set; }
    }
}
