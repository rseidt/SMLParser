using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public enum SMLUnit
    {
        W = 27, // active power (P) watt W = J/s
        Wh = 30 // active energy rW, active energy meter, constant or pulse value, watt-hour W*(60*60s)
                // More units in IEC 62056-62
    }
}
