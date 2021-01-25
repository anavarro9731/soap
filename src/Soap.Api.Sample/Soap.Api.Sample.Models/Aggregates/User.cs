namespace Soap.Api.Sample.Models.Aggregates
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;
    using Soap.Interfaces;

    public class User : Aggregate
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }
        
        public List<IUserChannelInfo> UserChannelInfo { get; set; }
        
        public string Auth0Id { get; set; }
    }
}
