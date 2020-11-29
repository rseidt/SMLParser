using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    class RawCrc:RawRecord
    {
        public ushort CRC { get; set; }
        public override int Length => 2;
    }
}
