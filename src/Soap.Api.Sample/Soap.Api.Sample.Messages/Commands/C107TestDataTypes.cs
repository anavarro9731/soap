namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using Soap.Interfaces.Messages;

    public class C107TestDataTypes : ApiMessage
    {
        public bool Boolean { get; set; }
        
        public bool? BooleanOptional { get; set; }

        public override ApiPermission Permission { get; }

        public string String { get; set; }
        
        public string? StringOptional { get; set; }
        
        public int Integer { get; set; }
        
        public int? IntegerOptional { get; set; }
        
        public short Short { get; set; }
        
        public short? ShortOptional { get; set; }
        
        public byte Byte { get; set; }
        
        public byte? ByteOptional { get; set; }
        
        public long Long { get; set; }

        public long? LongOptional { get; set; }
        
        public sbyte ShortByte { get; set; }
        
        public sbyte? ShortByteOptional { get; set; }
        
        public decimal Decimal { get; set; }
        
        public decimal? DecimalOptional { get; set; }
        
        public Guid Guid { get; set; }
        
        public Guid? GuidOptional { get; set; }
        
        public DateTime DateTime { get; set; }
        
        public DateTime? DateTimeOptional { get; set; }
    }
}