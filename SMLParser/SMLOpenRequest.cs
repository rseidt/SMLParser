using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class SMLOpenRequest : SMLMessageBody
    {
        public string CodePage { get; set; }
        public byte[] ClientId { get; set; }
        public byte[] ReqFileId { get; set; }
        public byte[] ServerId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public byte? SmlVersion { get; set; }
    }
}
