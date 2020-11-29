using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class SMLParser
    {
        internal static readonly byte[] beginIndicator = { 0x1b, 0x1b, 0x1b, 0x1b, 0x01, 0x01, 0x01, 0x01 };
        internal static readonly byte[] endIndicator = { 0x1b, 0x1b, 0x1b, 0x1b, 0x1a };

        private byte[] _binData = null;
        public byte[] BinaryRawData {
            get { if (_binData == null) throw new SMLException("Call 'ParseBinary' first, before accessing the BinaryRawData Property"); else return _binData; }
            private set { this._binData = value; }
        }
        private byte[] Message;
        int index = 0;
        public SMLParser(byte[] Message)
        {
            this.Message = Message;
        }

        public List<SMLMessage> Parse()
        {
            RawRecord record = ParseBinary();
            // structure well formed and crc ok.

            // expecting a response message

            if (record.Type != RawType.List && record.Length < 3)
            {
                throw new SMLException("expecting a sequence with at least 3 elements");
            }
            List<SMLMessage> document = new List<SMLMessage>();
            SMLInterpreter i = new SMLInterpreter();
            foreach (RawRecord message in ((RawSequence)record).Items)
            {
                SMLMessage m = i.ConvertToSmlMessage(message);
                document.Add(m);
            }
            return document;
        }

        public RawRecord ParseBinary()
        {

            int[] beginIndicators = Message.Locate(beginIndicator);
            if (beginIndicators.Length < 1)
                throw new SMLException("Could not find begin sequence");
            int[] endIndicators = Message.Locate(endIndicator);
            if (endIndicators.Length < 1)
                throw new SMLException("Could not find end sequence");
            int messageEndIndex = endIndicators[0];
            int messageStartIndex = beginIndicators[0];
            foreach (int endIndex in endIndicators)
            {
                if (endIndex > messageStartIndex)
                {
                    messageEndIndex = endIndex;
                    break;
                }
            }
            if (!(messageEndIndex > messageStartIndex && Message.Length >= messageEndIndex + 8))
            {
                throw new SMLException("Unable to find a complete Message");
            }
            messageEndIndex += 7;
            int dataStartIndex = beginIndicators[0] + beginIndicator.Length;
            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(Message, messageEndIndex - 1, 2);
            ushort checksum = BitConverter.ToUInt16(new byte[] { s[1], s[0] },0);
            ReadOnlySpan<byte> messageWithoutChecksum = new ReadOnlySpan<byte>(Message, messageStartIndex, messageEndIndex-messageStartIndex-1);
            Crc16 crc = new Crc16(0xFFFF, 0x1021, true, true, 0xFFFF);
            ushort calculatedSum = crc.ComputeChecksum(messageWithoutChecksum.ToArray());
            if (checksum != calculatedSum)
            {
                throw new SMLException("CRC check failed, message is corrupt.");
            }

            ReadOnlySpan<byte> strippedMessage = new ReadOnlySpan<byte>(Message, messageStartIndex, messageEndIndex - messageStartIndex + 1);
            this.BinaryRawData = strippedMessage.ToArray();

            RawSequence result = new RawSequence();
            index = dataStartIndex;
            while (Message[index].GetHighNibble() == 0x07)
            {
                var length = Message[index].GetLowNibble();
                result.Items.Add(new RawSequence (ParseList()));
            }
            result.Type = RawType.List;
            return result;
        }

        private uint ReadLength()
        {
            var type = Message[index].GetHighNibble();
            bool isList = type == (byte)RawType.List || type == (byte)RawType.LongList;
            uint length = 0;
            var bitindex = 0;
            ushort lengthBytes = 0;
            do
            {
                type = Message[index].GetHighNibble();
                uint lengthPart = (uint)Message[index].GetLowNibble();
                length = (length << bitindex) | lengthPart;
                bitindex += 4;
                lengthBytes++;
                index++;
                
            } while (type > 7);
            if (!isList && length >= lengthBytes)
                length -= lengthBytes;
            return length;
        }

        private List<RawRecord> ParseList()
        {
            var type = Message[index].GetHighNibble();
            if (type != (byte)RawType.List && type != (byte)RawType.LongList)
                throw new SMLException("Current byte ("+ Message[index].ToString("x2") +") does not point to a List indicator (0x7X | 0xFX)");
            var listLength = ReadLength();
            List<RawRecord> result = new List<RawRecord>();
            for (int i = 0; i < listLength; i++)
            {
                if (Message[index].GetHighNibble() == (byte)RawType.List || Message[index].GetHighNibble() == (byte)RawType.LongList) //SubList
                {
                    result.Add(new RawSequence(ParseList()));
                } else
                {
                    result.Add(ParseValue());
                }
            }
            return result;
        }

        private SMLValue ParseValue()
        {
            SMLValue val = new SMLValue();
            
            val.Type = (RawType)Message[index].GetHighNibble();
            var length = ReadLength();
            byte[] value = new byte[length];

            for (int i = 0; i < length; i++)
            {
                value[i] = Message[index];
                index++;
            }
            if (BitConverter.IsLittleEndian && (val.Type == RawType.Integer || val.Type == RawType.LongInteger || val.Type == RawType.LongUnsigned || val.Type == RawType.Unsigned || val.Type == RawType.Unsigned))
            {
                Array.Reverse(value);
            }

            val.Value = value;

            return val;
        }
    }
}
