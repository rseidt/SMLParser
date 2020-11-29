using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class SMLValue : RawRecord
    {
        public byte[] Value { get; set; }
        public override int Length { get { return Value == null ? 0: Value.Length; } }
    }
}
