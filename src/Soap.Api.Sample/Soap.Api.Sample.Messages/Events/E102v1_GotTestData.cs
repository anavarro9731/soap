namespace Soap.Api.Sample.Messages.Events
{
    using System;
    using CircuitBoard;
    using Soap.Api.Sample.Messages.Commands.UI;
    using Soap.Interfaces.Messages;

    public class E102v1_GotTestData : ApiEvent
    {
        public TestData E102_TestData { get; set; }

        public class TestData 
        {
            public bool? E102_Boolean { get; set; }

            public bool? E102_BooleanOptional { get; set; }

            public Address E102_CustomObject { get; set; }

            public DateTime? E102_DateTime { get; set; }

            public DateTime? E102_DateTimeOptional { get; set; }

            public decimal? E102_Decimal { get; set; }

            public decimal? E102_DecimalOptional { get; set; }

            public BlobMeta E102_File { get; set; }

            public BlobMeta E102_FileOptional { get; set; }

            public Guid? E102_Guid { get; set; }

            public Guid? E102_GuidOptional { get; set; }

            public BlobMeta E102_Image { get; set; }

            public BlobMeta E102_ImageOptional { get; set; }

            public long? E102_Long { get; set; }

            public long? E102_LongOptional { get; set; }

            public EnumerationAndFlags E102_PostCodesMulti { get; set; }

            public EnumerationAndFlags E102_PostCodesMultiOptional { get; set; }

            public EnumerationAndFlags E102_PostCodesSingle { get; set; }

            public EnumerationAndFlags E102_PostCodesSingleOptional { get; set; }

            public string E102_String { get; set; }

            public string E102_StringOptional { get; set; }

            public string E102_StringOptionalMultiline { get; set; }

            public class Address
            {
                public string E102_House { get; set; }

                public string E102_PostCode { get; set; }

                public string E102_Town { get; set; }
            }
        }

        public override void Validate()
        {
            
        }
    }
}
