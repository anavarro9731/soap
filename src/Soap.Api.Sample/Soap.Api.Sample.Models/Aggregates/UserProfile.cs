namespace Soap.Api.Sample.Models.Aggregates
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;
    using Soap.Interfaces;

    public class UserProfile : Aggregate, IUserProfile
    {
        public string Auth0Id { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
        
        public string UserName { get; set; }
    }
}
