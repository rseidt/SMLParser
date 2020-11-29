using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class SMLClose : SMLMessageBody
    {
        public byte[] GlobalSignature { get; set; }
    }
}
