using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public static class RawConvert
    {

        public const string defaultCodepage = "iso-8859-15";
        private static string _codePage = null;
        public static string CodePage {
            get { if (_codePage == null) return defaultCodepage; else return _codePage; }
            private set { _codePage = value; }
        }

        public const SMLDateTimeType defaultDateTimeType = SMLDateTimeType.SecIndex;
        private static SMLDateTimeType? _dateTimeType;
        public static SMLDateTimeType DateTimeType
        {
            get { if (_dateTimeType.HasValue) return _dateTimeType.Value; else return defaultDateTimeType; }
            private set { _dateTimeType = value; }
        }

        public static RawRecord MessageToSequence(SMLMessage message)
        {
            var groupBytes = new byte[] { message.GroupNo };
            var aoeBytes = new byte[] { (byte)message.AbortOnError };
            var typeBytes = BitConverter.GetBytes((uint)message.Body.Type);
            RawSequence bodyEnv = new RawSequence();

            bodyEnv.Items.Add(new SMLValue { Type = RawType.Unsigned, Value = typeBytes });
            bodyEnv.Items.Add(BodyToSequence(message.Body));
            

            RawSequence result = new RawSequence();
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = message.TransactionId });
            result.Items.Add(new SMLValue() { Type = RawType.Unsigned, Value = groupBytes });
            result.Items.Add(new SMLValue() { Type = RawType.Unsigned, Value = aoeBytes });
            result.Items.Add(bodyEnv);
            result.Items.Add(new RawCrc());
            result.Items.Add(new RawMessageEnd());

            return result;
        }

        public static RawSequence BodyToSequence(SMLMessageBody body)
        {
            switch (body.Type)
            {
                case SMLMessageType.ActionCosemRequest:
                    return ActionCosemRequestToSequence();
                case SMLMessageType.ActionCosemResponse:
                    return ActionCosemResponseToSequence();
                case SMLMessageType.AttensionResponse:
                    return AttensionResponseToSequence();
                case SMLMessageType.CloseRequest:
                case SMLMessageType.CloseResponse:
                    return CloseToSequence(body as SMLClose);
                case SMLMessageType.GetCosemRequest:
                    return GetCosemRequestToSequence();
                case SMLMessageType.GetCosemResponse:
                    return GetCosemResponseToSequence();
                case SMLMessageType.GetListRequest:
                    return GetListRequestToSequence(body as SMLGetListRequest);
                case SMLMessageType.GetListResponse:
                    return GetListResponseToSequence(body as SMLGetListResponse);
                case SMLMessageType.GetProcParameterRequest:
                    return GetProcParameterRequestToSequence();
                case SMLMessageType.GetProcParameterResponse:
                    return GetProcParameterResponseToSequence();
                case SMLMessageType.GetProfileListRequest:
                    return GetProfileListRequestToSequence();
                case SMLMessageType.GetProfileListResponse:
                    return GetProfileListResponseToSequence();
                case SMLMessageType.GetProfilePackRequest:
                    return GetProfilePackRequestToSequence();
                case SMLMessageType.GetProfilePackResponse:
                    return GetProfilePackResponseToSequence();
                case SMLMessageType.OpenRequest:
                    return OpenRequestToSequence(body as SMLOpenRequest);
                case SMLMessageType.OpenResponse:
                    return OpenResponseToSequence(body as SMLOpenResponse);
                case SMLMessageType.SetCosemRequest:
                    return SetCosemRequestToSequence();
                case SMLMessageType.SetCosemResponse:
                    return SetCosemResponseToSequence();
                case SMLMessageType.SetProcParameterRequest:
                    return SetProcParameterRequestToSequence();
                default:
                    throw new ApplicationException("Unsupported Message Type: " + (ushort)body.Type);

            }
        }

        private static RawSequence SetProcParameterRequestToSequence()
        {
            throw new NotImplementedException();
        }

        private static RawSequence SetCosemResponseToSequence()
        {
            throw new NotImplementedException();
        }

        private static RawSequence SetCosemRequestToSequence()
        {
            throw new NotImplementedException();
        }

        private static RawRecord TimestampToSequence(DateTime? Timestamp)
        {
            if (!Timestamp.HasValue)
            {
                return new SMLValue() { Type = RawType.OctetStream, Value = new byte[0] };
            } else
            {
                RawSequence result = new RawSequence();
                result.Items.Add(new SMLValue { Type = RawType.Unsigned, Value = new byte[] { (byte)DateTimeType } });
                switch (DateTimeType)
                {
                    case SMLDateTimeType.SecIndex:
                        byte[] secindex = BitConverter.GetBytes((uint)Timestamp.Value.Subtract(DateTime.MinValue).TotalSeconds);
                        result.Items.Add(new SMLValue() { Value = secindex, Type = RawType.Unsigned });
                        break;
                    case SMLDateTimeType.Timestamp:
                        byte[] utctime = BitConverter.GetBytes(Timestamp.Value.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
                        result.Items.Add(new SMLValue() { Value = utctime, Type = RawType.Unsigned });
                        break;
                    case SMLDateTimeType.LocalTimestamp:
                        utctime = BitConverter.GetBytes(Timestamp.Value.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
                        byte[] localOffset = BitConverter.GetBytes((short)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes);
                        byte[] seasonOffset = BitConverter.GetBytes((short)(TimeZoneInfo.Local.GetUtcOffset(Timestamp.Value).TotalMinutes - TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes));
                        RawSequence timeSequence = new RawSequence();
                        
                        timeSequence.Items.Add(new SMLValue { Value = utctime, Type = RawType.Unsigned });
                        timeSequence.Items.Add(new SMLValue { Value = localOffset, Type = RawType.Integer });
                        timeSequence.Items.Add(new SMLValue { Value = seasonOffset, Type = RawType.Integer });
                        result.Items.Add(timeSequence);
                        break;
                    default:
                        throw new NotSupportedException("DateTime Type " + DateTimeType + " is not supported.");
                }
                return result;

            }
        }

        private static RawSequence OpenResponseToSequence(SMLOpenResponse body)
        {
            RawSequence result = new RawSequence();
            RawRecord refTime = TimestampToSequence(body.RefTime);
            var codepage = Encoding.GetEncoding(defaultCodepage).GetBytes(body.CodePage);
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = codepage });
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = body.ClientId });
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = body.ReqFileId });
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = body.ServerId });
            result.Items.Add(refTime);
            result.Items.Add(new SMLValue { Type = RawType.OctetStream, Value = body.SmlVersion.HasValue ? new byte[] { body.SmlVersion.Value } : new byte[0] });
            return result;
        }

        private static RawSequence OpenRequestToSequence(SMLOpenRequest body)
        {
            RawSequence result = new RawSequence();
            var codepage = body.CodePage == null ? new byte[0] : Encoding.GetEncoding(defaultCodepage).GetBytes(body.CodePage);
            var username = body.Username == null ? new byte[0] : Encoding.GetEncoding(CodePage).GetBytes(body.Username);
            var password = body.Password == null ? new byte[0] : Encoding.GetEncoding(CodePage).GetBytes(body.Password);
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = codepage });
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = body.ClientId });
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = body.ReqFileId });
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = body.ServerId });
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = username });
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = password });
            result.Items.Add(new SMLValue { Type = RawType.Unsigned, Value = body.SmlVersion.HasValue ? new byte[] { body.SmlVersion.Value } : new byte[0] });
            return result;
        }

        private static RawSequence GetProfilePackResponseToSequence()
        {
            throw new NotImplementedException();
        }

        private static RawSequence GetProfilePackRequestToSequence()
        {
            throw new NotImplementedException();
        }

        private static RawSequence GetProfileListResponseToSequence()
        {
            throw new NotImplementedException();
        }

        private static RawSequence GetProfileListRequestToSequence()
        {
            throw new NotImplementedException();
        }

        private static RawSequence GetProcParameterResponseToSequence()
        {
            throw new NotImplementedException();
        }

        private static RawSequence GetProcParameterRequestToSequence()
        {
            throw new NotImplementedException();
        }

        private static RawSequence GetListResponseToSequence(SMLGetListResponse body)
        {
            RawSequence result = new RawSequence();
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = body.ClientId });
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = body.ServerId });
            result.Items.Add(new SMLValue { Type = RawType.OctetStream, Value = body.ListName });
            result.Items.Add(TimestampToSequence(body.ActSensorTime));
            result.Items.Add(ValListToSequence(body.ValList));
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = body.ListSignature });
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = body.ActGatewayInfo });
            return result;
        }
        
        private static RawRecord ValListToSequence(List<SMLListEntry> valList)
        {
            RawSequence result = new RawSequence();
            foreach(var meterValue in valList)
            {
                RawSequence item = new RawSequence();
                byte[] status;
                if (meterValue.status.HasValue)
                {
                    if (meterValue.status.Value > 0xFFFFFFFF)
                    {
                        status = BitConverter.GetBytes(meterValue.status.Value);
                    } else if(meterValue.status.Value > 0xFFFF) {
                        status = BitConverter.GetBytes((uint)meterValue.status.Value);
                    }
                    else if (meterValue.status.Value > 0xFF)
                    {
                        status = BitConverter.GetBytes((ushort)meterValue.status.Value);
                    }
                    else
                    {
                        status = BitConverter.GetBytes((byte)meterValue.status.Value);
                    }
                } else
                {
                    status = new byte[0];
                }

                var unit = meterValue.Unit.HasValue ? new byte[] { (byte)meterValue.Unit.Value } : new byte[0];
                var scaler = meterValue.Scaler.HasValue ? new byte[] { (byte)meterValue.Scaler.Value } : new byte[0];
                item.Items.Add(new SMLValue { Type = RawType.OctetStream, Value = meterValue.ObjName });
                item.Items.Add(new SMLValue { Type = status.Length > 0 ? RawType.Unsigned : RawType.OctetStream, Value = status });
                item.Items.Add(TimestampToSequence(meterValue.ValTime));
                item.Items.Add(new SMLValue { Type = unit.Length > 0 ? RawType.Unsigned : RawType.OctetStream, Value = unit });
                item.Items.Add(new SMLValue { Type = scaler.Length > 0 ? RawType.Integer : RawType.OctetStream, Value = scaler });
                item.Items.Add(meterValue.Value);
                item.Items.Add(new SMLValue { Type = RawType.OctetStream, Value = meterValue.ValueSignature });
                result.Items.Add(item);
            }
            return result;
        }

        private static RawSequence GetListRequestToSequence(SMLGetListRequest body)
        {
            RawSequence result = new RawSequence();
            var username = body.Username == null ? new byte[0] : Encoding.GetEncoding(CodePage).GetBytes(body.Username);
            var password = body.Password == null ? new byte[0] : Encoding.GetEncoding(CodePage).GetBytes(body.Password);
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = body.ClientId });
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = body.ServerId });
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = username });
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = password });
            result.Items.Add(new SMLValue { Type = RawType.Unsigned, Value = body.ListName });
            return result;
        }

        private static RawSequence GetCosemRequestToSequence()
        {
            throw new NotImplementedException();
        }

        private static RawSequence GetCosemResponseToSequence()
        {
            throw new NotImplementedException();
        }

        private static RawSequence CloseToSequence(SMLClose body)
        {
            RawSequence result = new RawSequence();
            result.Items.Add(new SMLValue() { Type = RawType.OctetStream, Value = body.GlobalSignature });
            return result;
        }

        private static RawSequence AttensionResponseToSequence()
        {
            throw new NotImplementedException();
        }

        private static RawSequence ActionCosemResponseToSequence()
        {
            throw new NotImplementedException();
        }

        private static RawSequence ActionCosemRequestToSequence()
        {
            throw new NotImplementedException();
        }

    }
}
