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

        public class ChildB
        {
            public bool? E105_BChildBool { get; set; }

            public Guid? E105_BChildId { get; set; }

            public long? E105_BChildLong { get; set; }

            public string E105_BChildString { get; set; }
            
        }

        public class ChildC
        {
            public ChildB E105_BChild { get; set; } = new ChildB();

            public List<ChildB> E105_BChildren { get; set; } = new List<ChildB>();

            public Guid? E105_CChildId { get; set; }

            public string E105_CChildString { get; set; }
        }

        public class TestData
        {
            public ChildC E105_CChild { get; set; } = new ChildC();
            
            public List<ChildC> E105_CChildren { get; set; } = new List<ChildC>();

            public string E105_Html { get; set; }
            
            public DateTime? E105_CreatedAt { get; set; }

            public Guid? E105_Guid { get; set; }

            public Guid? E105_Id { get; set; }

            public string E105_Label { get; set; }
            
        }
    }
}