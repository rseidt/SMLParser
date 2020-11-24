using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class SMLGetListResponse : SMLMessageBody
    {
        public string ClientId { get; set; }
        public string ServerId { get; set; }
        public string ListName { get; set; }
        public DateTime? ActSensorTime { get; set; }
        public List<SMLListEntry> ValList { get; set; }
        public string ListSignature { get; set; }
        public string ActGatewayInfo { get; set; }

    }
}
