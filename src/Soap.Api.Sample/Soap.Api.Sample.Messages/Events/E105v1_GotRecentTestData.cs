namespace Soap.Api.Sample.Messages.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Soap.Interfaces.Messages;

    public class E105v1_GotRecentTestData : ApiEvent
    {
        public List<TestData> E105_TestData { get; set; }

        public override void Validate()
        {
            
        }

        //TODO move these hardcoded fields to be created by default, also something in cached schema doesn't allow same name of field between root and child, but between children is ok
        
        
        public class ChildB
        {
            public bool? E105_Bool { get; set; } = true;

            public Guid? E105_Id { get; set; } = Guid.NewGuid();

            public long? E105_Long { get; set; } = 12345;

            public long? E105_String { get; set; } = 435435;
        }

        public class ChildC
        {
            public ChildB E105_Child2 { get; set; } = new ChildB();

            public List<ChildB> E105_Children2 { get; set; } = Enumerable.Range(1, 10).Select(x => new ChildB()).ToList();

            public Guid? E105_Id { get; set; } = Guid.NewGuid();

            public string E105_String { get; set; } = "test string";
        }

        public class TestData
        {
            public ChildC E105_Child { get; set; } = new ChildC();

            public List<ChildC> E105_Children { get; set; } = Enumerable.Range(1, 10).Select(x => new ChildC()).ToList();

            public DateTime? E105_CreatedAt { get; set; }

            public Guid? E105_Guid { get; set; }

            public Guid? E105_Id { get; set; }

            public string E105_Label { get; set; }
        }
    }
}