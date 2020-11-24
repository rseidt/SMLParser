using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class SMLMessage
    {
        public string TransactionId { get; set; }
        public byte GroupNo { get; set; }
        public SMLErrorHandling AbortOnError { get; set; }
        public SMLMessageBody Body { get; set; }
        public ushort CRC { get; set; }
    }
}
