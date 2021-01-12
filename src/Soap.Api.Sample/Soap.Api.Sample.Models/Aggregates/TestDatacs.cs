namespace Soap.Api.Sample.Models.Aggregates
{
    using System;
    using CircuitBoard;
    using DataStore.Interfaces.LowLevel;
    using Soap.Api.Sample.Models.ValueTypes;
    using Soap.Interfaces.Messages;

    public class TestData : Aggregate
    {
        public bool? Boolean { get; set; }

        public bool? BooleanOptional { get; set; }

        public Address CustomObject { get; set; }

        public DateTime? DateTime { get; set; }

        public DateTime? DateTimeOptional { get; set; }

        public decimal? Decimal { get; set; }

        public decimal? DecimalOptional { get; set; }

        public BlobMeta File { get; set; }

        public BlobMeta FileOptional { get; set; }

        public Guid? Guid { get; set; }

        public Guid? GuidOptional { get; set; }

        public BlobMeta Image { get; set; }

        public BlobMeta ImageOptional { get; set; }

        public long? Long { get; set; }

        public long? LongOptional { get; set; }

        public EnumerationAndFlags PostCodesMulti { get; set; }

        public EnumerationAndFlags PostCodesMultiOptional { get; set; }

        public EnumerationAndFlags PostCodesSingle { get; set; }

        public EnumerationAndFlags PostCodesSingleOptional { get; set; }

        public string String { get; set; }

        public string StringOptional { get; set; }

        public string StringOptionalMultiline { get; set; }
    }
}
