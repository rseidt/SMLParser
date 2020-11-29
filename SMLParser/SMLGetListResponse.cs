using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class SMLGetListResponse : SMLMessageBody
    {
        public byte[] ClientId { get; set; }
        public byte[] ServerId { get; set; }
        public byte[] ListName { get; set; }
        public DateTime? ActSensorTime { get; set; }
        public List<SMLListEntry> ValList { get; set; }
        public byte[] ListSignature { get; set; }
        public byte[] ActGatewayInfo { get; set; }

    }
}
