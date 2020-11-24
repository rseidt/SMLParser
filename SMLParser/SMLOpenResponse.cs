using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class SMLOpenResponse : SMLMessageBody
    {
        public string CodePage { get; set; }
        public string ClientId { get; set; }
        public string ReqFileId { get; set; }
        public string ServerId { get; set; }
        public DateTime? RefTime { get; set; }
        public byte? SmlVersion { get; set; }
    }
}
