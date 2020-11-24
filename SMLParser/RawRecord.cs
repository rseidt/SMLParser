using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public abstract class RawRecord
    {
        public int Length { get; set; }
        public RawType Type { get; set; }
    }
}
