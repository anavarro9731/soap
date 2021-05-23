namespace Soap.Api.Sample.Models.Entities
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;

    public class TestChildC : Entity
    {
        public TestChildB BChild { get; set; }

        public List<TestChildB> BChildren { get; set; }

        public string String { get; set; }
    }
}