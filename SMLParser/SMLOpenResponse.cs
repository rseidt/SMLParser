﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class SMLOpenResponse : SMLMessageBody
    {
        public string CodePage { get; set; }
        public byte[] ClientId { get; set; }
        public byte[] ReqFileId { get; set; }
        public byte[] ServerId { get; set; }
        public DateTime? RefTime { get; set; }
        public byte? SmlVersion { get; set; }
    }
}
