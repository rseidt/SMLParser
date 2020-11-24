using System;
using System.Collections.Generic;
using System.Text;

namespace SMLParser
{
    public class ObisCode
    {
        private static Dictionary<ulong, ObisCode> codes = new System.Collections.Generic.Dictionary<ulong, ObisCode> {
            { RegisterToKey("1-0:1.8.2*255"), new ObisCode("1-0:1.8.2*255", "Active Energy Tariff 2 Import") },
            { RegisterToKey("1-0:1.8.3*255"), new ObisCode("1-0:1.8.3*255", "Active Energy Tariff 3 Import") },
            { RegisterToKey("1-0:1.8.4*255"), new ObisCode("1-0:1.8.4*255", "Active Energy Tariff 4 Import") },
            { RegisterToKey("1-0:2.8.0*255"), new ObisCode("1-0:2.8.0*255", "Active Energy Total Export") },
            { RegisterToKey("1-0:2.8.1*255"), new ObisCode("1-0:2.8.1*255", "Active Energy Tariff 1 Export") },
            { RegisterToKey("1-0:2.8.2*255"), new ObisCode("1-0:2.8.2*255", "Active Energy Tariff 2 Export") },
            { RegisterToKey("1-0:2.8.3*255"), new ObisCode("1-0:2.8.3*255", "Active Energy Tariff 3 Export") },
            { RegisterToKey("1-0:1.8.1*255"), new ObisCode("1-0:1.8.1*255", "Active Energy Tariff 1 Import") },
            { RegisterToKey("1-0:2.8.4*255"), new ObisCode("1-0:2.8.4*255", "Active Energy Tariff 4 Export") },
            { RegisterToKey("1-0:16.7.0*255"), new ObisCode("1-0:16.7.0*255", "Active Power Total(Import and Export)") },
            { RegisterToKey("1-0:1.7.0*255"), new ObisCode("1-0:1.7.0*255", "Active Power Total") },
            { RegisterToKey("1-0:21.7.0*255"), new ObisCode("1-0:21.7.0*255", "Active Power Phase L1") },
            { RegisterToKey("1-0:41.7.0*255"), new ObisCode("1-0:41.7.0*255", "Active Power Phase L2") },
            { RegisterToKey("1-0:61.7.0*255"), new ObisCode("1-0:61.7.0*255", "Active Power Phase L3") },
            { RegisterToKey("1-0:1.8.0*255"), new ObisCode("1-0:1.8.0*255", "Active Energy Total Import") },
            { RegisterToKey("1-0:3.8.0*255"), new ObisCode("1-0:3.8.0*255", "Reactive Energy Total Import") },
            { RegisterToKey("1-0:3.8.1*255"), new ObisCode("1-0:3.8.1*255", "Reactive Energy T1 Import") },
            { RegisterToKey("1-0:3.8.2*255"), new ObisCode("1-0:3.8.2*255", "Reactive Energy T2 Import") },
            { RegisterToKey("1-0:3.8.3*255"), new ObisCode("1-0:3.8.3*255", "Reactive Energy T3 Import") },
            { RegisterToKey("1-0:3.8.4*255"), new ObisCode("1-0:3.8.4*255", "Reactive Energy T4 Import") },
            { RegisterToKey("1-1:5.8.0*255"), new ObisCode("1-1:5.8.0*255", "Reactive Energy Q1") },
            { RegisterToKey("1-1:6.8.0*255"), new ObisCode("1-1:6.8.0*255", "Reactive Energy Q2") },
            { RegisterToKey("1-1:7.8.0*255"), new ObisCode("1-1:7.8.0*255", "Reactive Energy Q3") },
            { RegisterToKey("1-1:8.8.0*255"), new ObisCode("1-1:8.8.0*255", "Reactive Energy Q4") },
            { RegisterToKey("1-0:3.7.0*255"), new ObisCode("1-0:3.7.0*255", "Reactive Power Total") },
            { RegisterToKey("1-0:23.7.0*255"), new ObisCode("1-0:23.7.0*255", "Reactive Power Phase L1") },
            { RegisterToKey("1-0:43.7.0*255"), new ObisCode("1-0:43.7.0*255", "Reactive Power Phase L2") },
            { RegisterToKey("1-0:63.7.0*255"), new ObisCode("1-0:63.7.0*255", "Reactive Power Phase L3") },
            { RegisterToKey("1-0:9.7.0*255"), new ObisCode("1-0:9.7.0*255", "Apparent Power Total") },
            { RegisterToKey("1-0:32.7.0*255"), new ObisCode("1-0:32.7.0*255", "Voltage Phase L1") },
            { RegisterToKey("1-0:52.7.0*255"), new ObisCode("1-0:52.7.0*255", "Voltage Phase L2") },
            { RegisterToKey("1-0:72.7.0*255"), new ObisCode("1-0:72.7.0*255", "Voltage Phase L3") },
            { RegisterToKey("1-0:11.7.0*255"), new ObisCode("1-0:11.7.0*255", "Current Total") },
            { RegisterToKey("1-0:31.7.0*255"), new ObisCode("1-0:31.7.0*255", "Current Phase L1") },
            { RegisterToKey("1-0:51.7.0*255"), new ObisCode("1-0:51.7.0*255", "Current Phase L2") },
            { RegisterToKey("1-0:71.7.0*255"), new ObisCode("1-0:71.7.0*255", "Current Phase L3") },
            { RegisterToKey("1-0:33.7.0*255"), new ObisCode("1-0:33.7.0*255", "Power Factor(cos Phi) Phase L1") },
            { RegisterToKey("1-0:53.7.0*255"), new ObisCode("1-0:53.7.0*255", "Power Factor(cos Phi) Phase L2") },
            { RegisterToKey("1-0:73.7.0*255"), new ObisCode("1-0:73.7.0*255", "Power Factor(cos Phi) Phase L3") },
            { RegisterToKey("9-0:2.0.0*255"), new ObisCode("9-0:2.0.0*255", "Hot Water: Flow rate") },
            { RegisterToKey("9-0:1.0.0*255"), new ObisCode("9-0:1.0.0*255", "Hot Water: Volumen") },
            { RegisterToKey("8-0:2.0.0*255"), new ObisCode("8-0:2.0.0*255", "Cold Water: Flow rate") },
            { RegisterToKey("8-0:1.0.0*255"), new ObisCode("8-0:1.0.0*255", "Cold Water: Volumen") },
            { RegisterToKey("7-0:3.0.0*255"), new ObisCode("7-0:3.0.0*255", "Gas: Volumen") },
            { RegisterToKey("7-0:3.1.0*255"), new ObisCode("7-0:3.1.0*255", "Gas: Volumen corrected") },
            { RegisterToKey("7-0:43.15.0*255"), new ObisCode("7-0:43.15.0*255", "Gas: Flow Rate") },
            { RegisterToKey("6-0:1.0.0*255"), new ObisCode("6-0:1.0.0*255", "Heat: Energy") },
            { RegisterToKey("6-0:1.0.1*255"), new ObisCode("6-0:1.0.1*255", "Heat: Energy Tariff 1") },
            { RegisterToKey("6-0:1.0.2*255"), new ObisCode("6-0:1.0.2*255", "Heat: Energy Tariff 2") },
            { RegisterToKey("6-0:8.0.0*255"), new ObisCode("6-0:8.0.0*255", "Heat: Power") },
            { RegisterToKey("6-0:10.0.0*255"), new ObisCode("6-0:10.0.0*255", "Heat: Flow temperature(Vorlauf)") },
            { RegisterToKey("6-0:11.0.0*255"), new ObisCode("6-0:11.0.0*255", "Heat: Return temperature(Rücklauf)") },
            { RegisterToKey("6-0:9.0.0*255"), new ObisCode("6-0:9.0.0*255", "Heat: Flow rate(m3/h)") },
            { RegisterToKey("5-0:1.0.0*255"), new ObisCode("5-0:1.0.0*255", "Cold: Energy") },
            { RegisterToKey("5-0:1.0.1*255"), new ObisCode("5-0:1.0.1*255", "Cold: Energy Tariff 1") },
            { RegisterToKey("5-0:1.0.2*255"), new ObisCode("5-0:1.0.2*255", "Cold: Energy Tariff 2)") },
            { RegisterToKey("5-0:8.0.0*255"), new ObisCode("5-0:8.0.0*255", "Cold: Power") },
            { RegisterToKey("5-0:10.0.0*255"), new ObisCode("5-0:10.0.0*255", "Cold: Flow temperature(Vorlauf)") },
            { RegisterToKey("5-0:11.0.0*255"), new ObisCode("5-0:11.0.0*255", "Cold: Return temperature(Rücklauf)") },
            { RegisterToKey("1-0:96.1.0*255"), new ObisCode("1-0:96.1.0*255", "Meter Serial number") },
            { RegisterToKey("1-0:96.1.10*255"), new ObisCode("1-0:96.1.10*255", "Meter Name (Metering Point ID abstract)") },
            { RegisterToKey("1-0:96.9.0*255"), new ObisCode("1-0:96.9.0*255", "Temperature") },
            { RegisterToKey("1-0:96.14.0*255"), new ObisCode("1-0:96.14.0*255", "The Active Tariff") },
            { RegisterToKey("1-0:14.7.0*255"), new ObisCode("1-0:14.7.0*255", "Net Frequency") },
            { RegisterToKey("17-0:129.128.0*255"), new ObisCode("17-0:129.128.0*255", "Compressed Air Counter Value Total(Norm cubic meter)") },
            { RegisterToKey("17-0:140.128.0*255"), new ObisCode("17-0:140.128.0*255", "Compressed Air Flow Rate") }
        };

        private static ulong RegisterToKey(string Register)
        {
            string[] decString = Register.Split(new char[] { '-', ':', '.', '*' });
            byte[] longBytes = new byte[8];
            for (int i = 0; i < decString.Length; i++)
            {
                longBytes[i] = Convert.ToByte(decString[i]);
            }
            return BitConverter.ToUInt64(longBytes,0);
        }

        private static ulong ByteArrayToKey(byte[] Register)
        {
            byte[] longBytes = new byte[8];
            for (int i = 0; i < Register.Length; i++)
            {
                longBytes[i] = Register[i];
            }
            return BitConverter.ToUInt64(longBytes,0);
        }

        public static ObisCode GetBySMLBytes(byte[] RegisterBytes)
        {
            var key = ByteArrayToKey(RegisterBytes);
            if (codes.ContainsKey(key))
                return codes[ByteArrayToKey(RegisterBytes)];
            else
                return null;
        }

        public ObisCode(string Register, string Name)
        {
            this.Register = Register;
            this.Name = Name;
        }

        public string Register { get; set; }
        public string Name { get; set; }
        internal ulong DictKey {
            get 
            {
                return RegisterToKey(Register);
            }
        }
    }
}
