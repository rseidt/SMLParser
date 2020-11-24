using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public enum SMLErrorHandling
    {
        Continue = 0x00,
        ContinueWithNextGroup = 0x01,
        AbortAfterCurrentGroup = 0x02,
        Abort = 0xFF
    }
}
