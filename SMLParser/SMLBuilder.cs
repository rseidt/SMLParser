using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SMLParser
{
    public class SMLBuilder
    {
        private readonly MemoryStream binaryResult;
        private readonly IEnumerable<SMLMessage> document;
        private readonly RawRecord rawData;
        public SMLBuilder(IEnumerable<SMLMessage> Document)
        {
            binaryResult = new MemoryStream();
            document = Document;
            rawData = new RawSequence();
        }

        public byte[] Convert()
        {

            BuildRawRecords();
            binaryResult.Write(SMLParser.beginIndicator, 0, SMLParser.beginIndicator.Length);
            BuildBinaryResult();
            var fillBytesCount = 4 - (binaryResult.Length % 4);
            for (int i = 0; i < fillBytesCount; i++)
                binaryResult.WriteByte(0x00);
            binaryResult.Write(SMLParser.endIndicator, 0, SMLParser.endIndicator.Length);
            binaryResult.WriteByte((byte)fillBytesCount);
            Crc16 crc = new Crc16(0xFFFF, 0x1021, true, true, 0xFFFF);
            byte[] calculatedSum = BitConverter.GetBytes(crc.ComputeChecksum(binaryResult.ToArray()));
            Array.Reverse(calculatedSum);
            binaryResult.Write(calculatedSum, 0, calculatedSum.Length);
            var result = binaryResult.ToArray();
            binaryResult.SetLength(0);
            binaryResult.Position = 0;
            return result;

        }

        private static byte GetFromNibbles(byte Highnibble, byte Lownibble)
        {
            return (byte)((Highnibble << 4) | Lownibble);
        }

        private void BuildRawRecords()
        {
            foreach (SMLMessage message in document)
            {
                ((RawSequence)rawData).Items.Add(RawConvert.MessageToSequence(message));
            }
        }

        private void BuildBinaryResult()
        {
            foreach (var item in ((RawSequence)rawData).Items)
            {
                if (item is RawSequence)
                    AppendSequence(item as RawSequence);
            }
        }

        private void AppendSequence(RawSequence rawData)
        {
            byte type = rawData.Length > 7 ? (byte)RawType.LongList : (byte)RawType.List;
            rawData.StartIndexInStream = (uint)binaryResult.Position;
            WriteLeadingBytes(type, (ushort)rawData.Length);
            foreach (RawRecord item in rawData.Items)
            {
                if (item is RawSequence)
                {
                    AppendSequence((RawSequence)item);
                }
                else if (item is SMLValue)
                {
                    AppendValue((SMLValue)item);
                }
                else if (item is RawCrc)
                {
                    AppendCRC(rawData.StartIndexInStream.Value);
                }
                else if (item is RawMessageEnd)
                {
                    binaryResult.WriteByte(0x00);
                }

            }
        }

        private void AppendCRC(uint startIndexInStream)
        {
            long originalPosition = binaryResult.Position;

            byte[] buffer = new byte[originalPosition - startIndexInStream];
            binaryResult.Position = startIndexInStream;
            binaryResult.Read(buffer, 0, buffer.Length);

            Crc16 crc = new Crc16(0xFFFF, 0x1021, true, true, 0xFFFF);
            ushort calculatedSum = crc.ComputeChecksum(buffer);
            byte[] crcBytes = BitConverter.GetBytes(calculatedSum);
            Array.Reverse(crcBytes);
            binaryResult.WriteByte(0x63);
            binaryResult.Write(crcBytes, 0, crcBytes.Length);
        }

        private void AppendValue(SMLValue item)
        {

            WriteLeadingBytes((byte)item.Type, (ushort)item.Length);
            var bytesToWrite = item.Value;
            if (item.Type == RawType.Integer || item.Type == RawType.LongInteger || item.Type == RawType.LongUnsigned || item.Type == RawType.Unsigned)
            {
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytesToWrite);
            }
            binaryResult.Write(bytesToWrite, 0, bytesToWrite.Length);
        }

        private void WriteLeadingBytes(byte Type, ushort Length)
        {
            if (!BitConverter.IsLittleEndian)
            {
                var bytes = BitConverter.GetBytes(Length);
                Array.Reverse(bytes);
                Length = BitConverter.ToUInt16(bytes, 0);
            }

            if (Type != (byte)RawType.List && Type != (byte)RawType.LongList)
            {
                ushort LeadingBytesLength = 1;
                if (Length + LeadingBytesLength > 0x000F)
                    LeadingBytesLength++;
                if (Length + LeadingBytesLength > 0x00FF)
                    LeadingBytesLength++;
                if (Length + LeadingBytesLength > 0x0FFF)
                    LeadingBytesLength++;
                Length += LeadingBytesLength;
            }
            while ((Length & 0xF000) == 0)
                Length <<= 4;
            ushort bitindex = 0;
            do
            {
                byte LengthPart = (byte)(((Length << bitindex) & 0xF000) >> 12);
                binaryResult.WriteByte(GetFromNibbles(Type, LengthPart));
                Type = 0;
                bitindex += 4;
            } while (bitindex < 16 && (ushort)(Length << bitindex) > 0);
        }
    }
}
