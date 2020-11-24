using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class SMLInterpreter
    {
        public const string defaultCodepage = "iso-8859-15";
        private string messageCodePage = null;
        public string MessageCodepage {
            get
            {
                if (messageCodePage == null)
                    return defaultCodepage;
                else
                    return messageCodePage;
            } private set
            {
                messageCodePage = value;
            }
        }


        public SMLInterpreter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public SMLMessage ConvertToSmlMessage(RawRecord record)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            SMLMessage result = new SMLMessage();
            var l = record as RawSequence;
            result.AbortOnError = (SMLErrorHandling)((SMLValue)l.Items[1]).Value[0];
            result.GroupNo = ((SMLValue)l.Items[2]).Value[0];
            result.TransactionId = ConvertToByteString(((SMLValue)l.Items[0]).Value);
            result.CRC = BitConverter.ToUInt16(((SMLValue)l.Items[4]).Value,0);
            result.Body = ConvertToMessageBody((RawSequence)l.Items[3]);
            return result;
        }

        private string ConvertToByteString(byte[] value)
        {
            string result = "";
            foreach (byte b in value)
                result += b.ToString("x2") + " ";
            return result.Trim();
        }

        private SMLMessageBody ConvertToMessageBody(RawSequence list)
        {
            if (list.Length != 2)
                throw new Exception("Message Body must consist of two items");
            SMLMessageType type = (SMLMessageType)BitConverter.ToUInt32(((SMLValue)list.Items[0]).Value,0);
            SMLMessageBody body;
            switch(type)
            {
                case SMLMessageType.OpenResponse:
                    body = ConvertToOpenResponse(list.Items[1] as RawSequence);
                    break;
                case SMLMessageType.CloseResponse:
                    body = ConvertToCloseResponse(list.Items[1] as RawSequence);
                    break;
                case SMLMessageType.GetListResponse:
                    body = ConvertToGetListResponse(list.Items[1] as RawSequence);
                    break;
                default:
                    throw new NotSupportedException("The Message Type " + type.ToString() + " is currently not suported.");
                    
            }
            body.Type = type;
            return body;
        }

        private SMLOpenResponse ConvertToOpenResponse(RawSequence list)
        {
            SMLOpenResponse result = new SMLOpenResponse();
            result.CodePage = Encoding.GetEncoding(MessageCodepage).GetString(((SMLValue)list.Items[0]).Value);
            if (!String.IsNullOrEmpty(result.CodePage))
            {
                MessageCodepage = result.CodePage;
            }
            result.ClientId = ConvertToByteString(((SMLValue)list.Items[1]).Value);
            result.ReqFileId = ConvertToByteString(((SMLValue)list.Items[2]).Value);
            result.ServerId = ConvertToByteString(((SMLValue)list.Items[3]).Value);
            result.RefTime = ConvertToDateTime((RawSequence)list.Items[4]);
            result.SmlVersion = ((SMLValue)list.Items[5]).Value.Length > 0 ? ((SMLValue)list.Items[5]).Value[0] : new Nullable<byte>();
            return result;
        }
        private SMLGetListResponse ConvertToGetListResponse(RawSequence list)
        {
            SMLGetListResponse result = new SMLGetListResponse();
            result.ClientId = ConvertToByteString(((SMLValue)list.Items[0]).Value);
            result.ServerId = ConvertToByteString(((SMLValue)list.Items[1]).Value);
            result.ListName = ConvertToByteString(((SMLValue)list.Items[2]).Value);
            result.ActSensorTime = ConvertToDateTime((RawSequence)list.Items[3]);
            result.ValList = ConvertToValList((RawSequence)list.Items[4]);
            result.ListSignature = Encoding.GetEncoding(MessageCodepage).GetString(((SMLValue)list.Items[5]).Value);
            result.ActGatewayInfo = ConvertToByteString(((SMLValue)list.Items[6]).Value);
            return result;
        }

        private List<SMLListEntry> ConvertToValList(RawSequence list)
        {
            List<SMLListEntry> result = new List<SMLListEntry>();
            foreach (RawSequence item in list.Items)
            {
                SMLListEntry entry = new SMLListEntry();
                entry.ObjName = ConvertToByteString(((SMLValue)item.Items[0]).Value);
                entry.status = item.Items[1].Length > 0 ? BitConverter.ToUInt64(ExtendTo64Bit(((SMLValue)item.Items[1]).Value),0): new Nullable<ulong>();
                entry.ValTime = ConvertToDateTime(item.Items[2] as RawSequence);
                entry.Unit = item.Items[3].Length > 0 ? (SMLUnit)((SMLValue)item.Items[3]).Value[0] : new Nullable<SMLUnit>();
                entry.Scaler = item.Items[4].Length > 0 ? Convert.ToInt16(((SMLValue)item.Items[4]).Value[0]) : new Nullable<short>();
                entry.Value = (SMLValue)item.Items[5];
                entry.IntValue = item.Items[5].Type == RawType.LongInteger || item.Items[5].Type == RawType.Integer ? BitConverter.ToInt64(ExtendTo64Bit(((SMLValue)item.Items[5]).Value),0) : new Nullable<long>();
                entry.UIntValue = item.Items[5].Type == RawType.LongUnsigned || item.Items[5].Type == RawType.Unsigned ? BitConverter.ToUInt64(ExtendTo64Bit(((SMLValue)item.Items[5]).Value),0) : new Nullable<ulong>();
                entry.StringValue = item.Items[5].Type == RawType.OctetStream || item.Items[5].Type == RawType.LongOctetStream ? Encoding.GetEncoding(MessageCodepage).GetString(((SMLValue)item.Items[5]).Value) : null;
                entry.ByteString = item.Items[5].Type == RawType.OctetStream || item.Items[5].Type == RawType.LongOctetStream ? ConvertToByteString(((SMLValue)item.Items[5]).Value) : null;
                entry.BoolValue = item.Items[5].Type == RawType.Boolean ?  ((SMLValue)item.Items[5]).Value[0] != 0x00 : new Nullable<bool>();
                entry.ObisCode = ObisCode.GetBySMLBytes(((SMLValue)item.Items[0]).Value);
                entry.ValueSignature = ((SMLValue)item.Items[6]).Value;
                result.Add(entry);
            }
            return result;
        }

        public byte[] ExtendTo64Bit(byte[] value)
        {
            byte[] result = new byte[8];
            for(int i = 0; i < result.Length; i++)
            {
                if (i < value.Length)
                    result[i] = value[i];
                else
                    result[i] = 0;
            }
            return result;

        }
        private DateTime? ConvertToDateTime(RawSequence list)
        {
            if (list == null)
            {
                return new Nullable<DateTime>();
            }
            var dtType = (SMLDateTimeType)((SMLValue)list.Items[0]).Value[0];
            switch (dtType)
            {
                case SMLDateTimeType.SecIndex:
                    return DateTime.MinValue.AddSeconds(BitConverter.ToUInt32(((SMLValue)list.Items[1]).Value,0));
                case SMLDateTimeType.Timestamp:
                    DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    result = result.AddSeconds(BitConverter.ToUInt32(((SMLValue)list.Items[1]).Value,0));
                    return result.ToLocalTime();
                case SMLDateTimeType.LocalTimestamp:
                    var dtOffset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.FromMinutes(BitConverter.ToInt16(((SMLValue)((RawSequence)list.Items[1]).Items[1]).Value, 0) + BitConverter.ToInt16(((SMLValue)((RawSequence)list.Items[1]).Items[2]).Value,0)));
                    result = dtOffset.LocalDateTime;
                    result = result.AddSeconds(BitConverter.ToUInt32(((SMLValue)((RawSequence)list.Items[1]).Items[0]).Value,0));
                    return result;
                default:
                    throw new NotSupportedException("DateTime Type " + ((SMLValue)list.Items[0]).Value[0].ToString("x2") + " is not supported.");
            }

        }

        private SMLPublicClose ConvertToCloseResponse(RawSequence list)
        {
            SMLPublicClose result = new SMLPublicClose();
            result.GlobalSignature = ((SMLValue)list.Items[0]).Value;
            return result;
        }


    }
}
