//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Models.Entities
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;

    public class TestChildC : Entity
    {
        public TestChildB CapitalCity { get; set; }

        public List<TestChildB> Cities { get; set; }

        public string String { get; set; }
    }
}