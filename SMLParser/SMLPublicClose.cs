using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class SMLPublicClose : SMLMessageBody
    {
        public byte[] GlobalSignature { get; set; }
    }
}
