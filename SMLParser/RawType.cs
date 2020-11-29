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
        Unsigned = 0x6,
        List = 0x7,
        LongList = 0xF,
        LongOctetStream =0x8,
        LongInteger = 0xD,
        LongUnsigned = 0xE
    }
}
