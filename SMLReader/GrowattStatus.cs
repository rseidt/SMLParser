using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMLReader
{
    public class OnlineStatus
    {
        public string InverterStatus { get; set;  } //: "Normal",
        public decimal DcVoltage { get; set; }
        public decimal AcFreq { get; set; }
        public decimal AcVoltage { get; set; }
        public decimal AcPower { get; set; }
        public decimal EnergyToday { get; set; }
        public decimal EnergyTotal { get; set; }
        public long OperatingTime { get; set; }
        public decimal Temperature { get; set; }
        public int AccumulatedEnergy { get; set; }
        public int Cnt { get; set; }
    }

    public class GrowattStatus
    {
        public int InverterStatus { get; set; }
        public decimal PV1EnergyTotal { get; set; }
        public decimal OutputPower { get; set; }

        //{"InverterStatus":0,"InputPower":0,"PV1Voltage":263.7,"PV1InputCurrent":0,"PV1InputPower":0,"PV2Voltage":0,"PV2InputCurrent":0,"PV2InputPower":0,"OutputPower":0,"GridFrequency":50.02,"L1ThreePhaseGridVoltage":229.7,"L1ThreePhaseGridOutputCurrent":0,"L1ThreePhaseGridOutputPower":0,"L2ThreePhaseGridVoltage":0,"L2ThreePhaseGridOutputCurrent":0,"L2ThreePhaseGridOutputPower":0,"L3ThreePhaseGridVoltage":0,"L3ThreePhaseGridOutputCurrent":0,"L3ThreePhaseGridOutputPower":0,"TodayGenerateEnergy":0,"TotalGenerateEnergy":0,"TWorkTimeTotal":0,"PV1EnergyToday":0,"PV1EnergyTotal":0,"PV2EnergyToday":0,"PV2EnergyTotal":0,"PVEnergyTotal":0,"InverterTemperature":12.6,"TemperatureInsideIPM":12.6,"BoostTemperature":0,"DischargePower":0,"ChargePower":0,"BatteryVoltage":0,"SOC":0,"ACPowerToUser":0,"ACPowerToUserTotal":0,"ACPowerToGrid":0,"ACPowerToGridTotal":0,"INVPowerToLocalLoad":0,"INVPowerToLocalLoadTotal":0,"BatteryTemperature":0,"BatteryState":0,"EnergyToUserToday":0,"EnergyToUserTotal":0,"EnergyToGridToday":0,"EnergyToGridTotal":0,"DischargeEnergyToday":0,"DischargeEnergyTotal":0,"ChargeEnergyToday":0,"ChargeEnergyTotal":0,"LocalLoadEnergyToday":0,"LocalLoadEnergyTotal":0,"Mac":"BC:FF:4D:57:2E:30","Cnt":113}
        //public OnlineStatus Status { get; set; }
    }
}
