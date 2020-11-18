namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using Soap.Interfaces.Messages;

    public class C107v1TestDataTypes : ApiMessage
    {
        public bool C107_Boolean { get; set; }
        
        public bool? C107_BooleanOptional { get; set; }

        public override ApiPermission Permission { get; }

        public string C107_String { get; set; }
        
        public string? C107_StringOptional { get; set; }
        
        public int C107_Integer { get; set; }
        
        public int? C107_IntegerOptional { get; set; }
        
        public short C107_Short { get; set; }
        
        public short? C107_ShortOptional { get; set; }
        
        public byte C107_Byte { get; set; }
        
        public byte? C107_ByteOptional { get; set; }
        
        public long C107_Long { get; set; }

        public long? C107_LongOptional { get; set; }
        
        public sbyte C107_ShortByte { get; set; }
        
        public sbyte? C107_ShortByteOptional { get; set; }
        
        public decimal C107_Decimal { get; set; }
        
        public decimal? C107_DecimalOptional { get; set; }
        
        public Guid C107_Guid { get; set; }
        
        public Guid? C107_GuidOptional { get; set; }
        
        public DateTime C107_DateTime { get; set; }
        
        public DateTime? C107_DateTimeOptional { get; set; }
    }
}