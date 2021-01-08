namespace Soap.Api.Sample.Messages.Events
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard;
    using Soap.Interfaces.Messages;

    public class E105v1_GotRecentTestData : ApiEvent
    {
        public List<TestData> E105_TestData { get; set; }

        public override void Validate()
        {
        }

        public class TestData 
        {
            public Guid? E105_Id { get; set; }

            public Guid? E105_Guid { get; set; }
            
            public DateTime? E105_CreatedAt { get; set; }

            public string E105_Label { get; set; }

        }
    }
}
