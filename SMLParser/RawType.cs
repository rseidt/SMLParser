using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public enum RawType
    {
        OctetStream = 0x0,
        Boolean = 0x4,
        Integer = 0x5,
        List = 0x7,
        LongList = 0x7,
        LongOctetStream =0x8,
        LongInteger = 0xD,
        Unsigned = 0x6,
        LongUnsigned = 0xE
    }
}
