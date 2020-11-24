using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public abstract class SMLMessageBody
    {
        public SMLMessageType Type { get; set; }
    }
}
