using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public abstract class RawRecord
    {
        public abstract int Length { get; }
        public RawType Type { get; set; }
    }
}
