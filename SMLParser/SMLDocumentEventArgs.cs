using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class SMLDocumentEventArgs : EventArgs
    {
        public List<SMLMessage> Document { get; internal set; }
        public byte[] BinaryRawMessage { get; internal set; }
    }
}
