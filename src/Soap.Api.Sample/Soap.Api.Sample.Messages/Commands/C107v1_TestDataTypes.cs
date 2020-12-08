namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard;
    using Soap.Api.Sample.Messages.Commands.UI;
    using Soap.Interfaces.Messages;

    public class C107v1_TestDataTypes : ApiCommand
    {
        //* see JSON.NET serialisation modes for how this is handled by default on .NET side

        [Required]
        public bool? C107_Boolean { get; set; }

        public bool? C107_BooleanOptional { get; set; }

        [Required]
        public DateTime? C107_DateTime { get; set; }

        public DateTime? C107_DateTimeOptional { get; set; }

        [Required]
        public decimal? C107_Decimal { get; set; }

        public decimal? C107_DecimalOptional { get; set; }

        [Required]
        public Guid? C107_Guid { get; set; }

        public Guid? C107_GuidOptional { get; set; }

        [Required]
        public long? C107_Long { get; set; }

        public long? C107_LongOptional { get; set; }

        [Required]
        public string? C107_String { get; set; }

        public string? C107_StringOptional { get; set; }
        
        [MultiLine]
        public string? C107_StringOptionalMultiline { get; set; }

        public Address C107_CustomObject { get; set; }
        
        [Required]
        public EnumerationAndFlags C107_PostCodesSingle { get; set; }
        
        public EnumerationAndFlags C107_PostCodesSingleOptional { get; set; }
        
        [Required]
        public EnumerationAndFlags C107_PostCodesMulti { get; set; }
        
        public EnumerationAndFlags C107_PostCodesMultiOptional { get; set; }

        public class Address
        {
            [Label("House Name")]
            public string? C107_House { get; set; }

            [Label("Post Code")]
            public string? C107_PostCode { get; set; }

            public string? C107_Town { get; set; }
        }
        
        [Required]
        [Base64(Base64Attribute.BlobType.Image)]
        public string? C107_Image { get; set; }
        
        [Base64(Base64Attribute.BlobType.Image)]
        public string? C107_ImageOptional { get; set; }
        
        
        [Required]
        [Base64(Base64Attribute.BlobType.File)]
        public string? C107_File { get; set; }
        
        [Base64(Base64Attribute.BlobType.File)]
        public string? C107_FileOptional { get; set; }
        
        
    }
}
