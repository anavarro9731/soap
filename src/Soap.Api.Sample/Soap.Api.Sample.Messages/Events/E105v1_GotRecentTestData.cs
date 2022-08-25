//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Messages.Events
{
    using System;
    using System.Collections.Generic;
    using Soap.Interfaces.Messages;

    public class E105v1_GotRecentTestData : ApiEvent
    {
        public List<TestData> E105_TestData { get; set; }

        public override void Validate()
        {
        }

        public class City
        {
            public bool? E105_HasCathedral { get; set; }

            public Guid? E105_CityId { get; set; }

            public long? E105_Population { get; set; }

            public string E105_Name { get; set; }
            
        }

        public class Country
        {
            public City E105_CapitalCity { get; set; } = new City();

            public List<City> E105_Cities { get; set; } = new List<City>();

            public Guid? E105_CountryId { get; set; }

            public string E105_Name2 { get; set; }
        }

        public class TestData
        {
            public List<Country> E105_Countries { get; set; } = new List<Country>();

            public string E105_Html { get; set; }
            
            public DateTime? E105_CreatedAt { get; set; }

            public Guid? E105_Guid { get; set; }

            public Guid? E105_Id { get; set; }

            public string E105_Label { get; set; }
            
        }
    }
}