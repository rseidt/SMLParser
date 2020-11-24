using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class Crc16
    {
        private ushort poly = 0;
        private ushort[] table = new ushort[256];
        private ushort initialValue = 0;
        private bool refIn;
        private bool refOut;
        private ushort xorOut;

        public ushort ComputeChecksum(byte[] bytes)
        {
            ushort crc = this.initialValue;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte b = refIn ? Reverse(bytes[i]) : bytes[i];
                crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & b))]);
            }


            return this.refOut ? (ushort)(Reverse(crc) ^ xorOut) : (ushort)(crc ^ xorOut);
        }

        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            ushort crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }

        private static byte Reverse(byte b)
        {
            return (byte)(((b * 0x80200802ul) & 0x0884422110ul) * 0x0101010101ul >> 32);
        }

        private static ushort Reverse(ushort i)
        {
            byte[] bytes = BitConverter.GetBytes(i);
            bytes[0] = Reverse(bytes[0]);
            bytes[1] = Reverse(bytes[1]);
            return BitConverter.ToUInt16(bytes,0);
        }

        public Crc16(ushort InitialValue, ushort Poly, bool RefIn, bool RefOut, ushort XorOut )
        {
            this.initialValue = InitialValue;
            this.poly = Poly;
            this.refIn = RefIn;
            this.refOut = RefOut;
            this.xorOut = XorOut;
            ushort temp, a;
            for (int i = 0; i < table.Length; ++i)
            {
                temp = 0;
                a = (ushort)(i << 8);
                for (int j = 0; j < 8; ++j)
                {
                    if (((temp ^ a) & 0x8000) != 0)
                    {
                        temp = (ushort)((temp << 1) ^ poly);
                    }
                    else
                    {
                        temp <<= 1;
                    }
                    a <<= 1;
                }
                table[i] = temp;
            }
        }
    }
}
