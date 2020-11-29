using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class SMLListEntry
    {
        public byte[] ObjName { get; set; }
        public ulong? status { get; set; }
        public DateTime? ValTime { get; set; }
        public SMLUnit? Unit { get; set; }
        public sbyte? Scaler { get; set; }
        public SMLValue Value { get; set; }
        public long? IntValue { get; set; }
        public ulong? UIntValue { get; set; }
        public string StringValue { get; set; }
        public string ByteString { get; set; }
        public bool? BoolValue { get; set; }
        public ObisCode ObisCode { get; set; }
        public byte[] ValueSignature { get; set; }
    }
}
