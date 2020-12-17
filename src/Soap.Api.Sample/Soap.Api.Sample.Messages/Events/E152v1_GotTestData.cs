namespace Soap.Api.Sample.Messages.Events
{
    using System;
    using CircuitBoard;
    using Soap.Api.Sample.Messages.Commands.UI;
    using Soap.Interfaces.Messages;

    public class E152v1_GoTestData : ApiEvent
    {
        public TestData E152_TestData { get; set; }

        public class TestData 
        {
            public bool? E152_Boolean { get; set; }

            public bool? E152_BooleanOptional { get; set; }

            public Address E152_CustomObject { get; set; }

            public DateTime? E152_DateTime { get; set; }

            public DateTime? E152_DateTimeOptional { get; set; }

            public decimal? E152_Decimal { get; set; }

            public decimal? E152_DecimalOptional { get; set; }

            public BlobMeta E152_File { get; set; }

            public BlobMeta E152_FileOptional { get; set; }

            public Guid? E152_Guid { get; set; }

            public Guid? E152_GuidOptional { get; set; }

            public BlobMeta E152_Image { get; set; }

            public BlobMeta E152_ImageOptional { get; set; }

            public long? E152_Long { get; set; }

            public long? E152_LongOptional { get; set; }

            public EnumerationAndFlags E152_PostCodesMulti { get; set; }

            public EnumerationAndFlags E152_PostCodesMultiOptional { get; set; }

            public EnumerationAndFlags E152_PostCodesSingle { get; set; }

            public EnumerationAndFlags E152_PostCodesSingleOptional { get; set; }

            public string E152_String { get; set; }

            public string E152_StringOptional { get; set; }

            public string E152_StringOptionalMultiline { get; set; }

            public class Address
            {
                public string E152_House { get; set; }

                public string E152_PostCode { get; set; }

                public string E152_Town { get; set; }
            }
        }

        public override void Validate()
        {
            
        }
    }
}
