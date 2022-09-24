using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMLReader
{
    public class GrowattStatus
    {
        public int InverterStatus { get; set; }
        public decimal InputPower { get; set; }
        public decimal PV1Voltage { get; set; }
        public decimal PV1InputCurrent { get; set; }
        public decimal PV1InputPower { get; set; }
        public decimal PV2Voltage { get; set; }
        public decimal PV2InputCurrent { get; set; }
        public decimal PV2InputPower { get; set; }
        public decimal OutputPower { get; set; }
        public decimal GridFrequency { get; set; }
        public decimal L1ThreePhaseGridVoltage { get; set; }
        public decimal L1ThreePhaseGridOutputCurrent { get; set; }
        public decimal L1ThreePhaseGridOutputPower { get; set; }
        public decimal L2ThreePhaseGridVoltage { get; set; }
        public decimal L2ThreePhaseGridOutputCurrent { get; set; }
        public decimal L2ThreePhaseGridOutputPower { get; set; }
        public decimal L3ThreePhaseGridVoltage { get; set; }
        public decimal L3ThreePhaseGridOutputCurrent { get; set; }
        public decimal L3ThreePhaseGridOutputPower { get; set; }
        public decimal TodayGenerateEnergy { get; set; }
        public decimal TotalGenerateEnergy { get; set; }
        public decimal TWorkTimeTotal { get; set; }
        public decimal PV1EnergyToday { get; set; }
        public decimal PV1EnergyTotal { get; set; }
        public decimal PV2EnergyToday { get; set; }
        public decimal PV2EnergyTotal { get; set; }
        public decimal PVEnergyTotal { get; set; }
        public decimal InverterTemperature { get; set; }
        public decimal TemperatureInsideIPM { get; set; }
        public decimal BoostTemperature { get; set; }
        public decimal DischargePower { get; set; }
        public decimal ChargePower { get; set; }
        public decimal BatteryVoltage { get; set; }
        public decimal SOC { get; set; }
        public decimal ACPowerToUser { get; set; }
        public decimal ACPowerToUserTotal { get; set; }
        public decimal ACPowerToGrid { get; set; }
        public decimal ACPowerToGridTotal { get; set; }
        public decimal INVPowerToLocalLoad { get; set; }
        public decimal INVPowerToLocalLoadTotal { get; set; }
        public decimal BatteryTemperature { get; set; }
        public decimal BatteryState { get; set; }
        public decimal EnergyToUserToday { get; set; }
        public decimal EnergyToUserTotal { get; set; }
        public decimal EnergyToGridToday { get; set; }
        public decimal EnergyToGridTotal { get; set; }
        public decimal DischargeEnergyToday { get; set; }
        public decimal DischargeEnergyTotal { get; set; }
        public decimal ChargeEnergyToday { get; set; }
        public decimal ChargeEnergyTotal { get; set; }
        public decimal LocalLoadEnergyToday { get; set; }
        public decimal LocalLoadEnergyTotal { get; set; }
        public string Mac { get; set; }
        public decimal Cnt { get; set; }

    }
}
