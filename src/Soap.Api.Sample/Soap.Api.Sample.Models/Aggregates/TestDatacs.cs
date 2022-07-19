namespace Soap.Api.Sample.Models.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Models.PartitionKeys;
    using Soap.Api.Sample.Models.Entities;
    using Soap.Api.Sample.Models.ValueTypes;
    using Soap.Interfaces.Messages;

    [PartitionKey__Shared]
    public class TestData : Aggregate
    {
        public bool? Boolean { get; set; }

        public bool? BooleanOptional { get; set; }

        public Address CustomObject { get; set; }

        public DateTime? DateTime { get; set; }

        public DateTime? DateTimeOptional { get; set; }

        public decimal? Decimal { get; set; }

        public decimal? DecimalOptional { get; set; }

        public TestChildC CChild { get; set; } 

        public List<TestChildC> CChildren { get; set; }

        public BlobMeta File { get; set; }

        public BlobMeta FileOptional { get; set; }

        public Guid? Guid { get; set; }

        public Guid? GuidOptional { get; set; }

        public BlobMeta Image { get; set; }

        public BlobMeta ImageOptional { get; set; }

        public long? Long { get; set; }

        public long? LongOptional { get; set; }

        public List<string> PostCodesMultiKeys { get; set; } = new List<string>();

        public List<string> PostCodesMultiOptionalKeys { get; set; } = new List<string>();

        public string PostCodesSingleKey { get; set; }

        public string PostCodesSingleOptionalKey { get; set; }

        public List<string> Hashtags { get; set; } = new List<string>();    
        
        public string String { get; set; }

        public string StringOptional { get; set; }

        public string StringOptionalMultiline { get; set; }
        
        public string HtmlOptionalMultiline { get; set; }
    }
}