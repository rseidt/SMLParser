using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class SMLGetListRequest : SMLMessageBody
    {
        public byte[] ClientId { get; set; }
        public byte[] ServerId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public byte[] ListName { get; set; }

    }
}
