using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Linq;
using SMLParser;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SMLReader
{
    internal class Currents
    {


        internal int? obis180;
        internal int? obis280;
        internal int? effectivePower;
    }



    class SMLReder
    {


        static ManualResetEvent quitEvent = new ManualResetEvent(false);

        static readonly Dictionary<string, MessagePipe> MessagePipes = new Dictionary<string, MessagePipe>();
        static readonly Dictionary<string, Currents> PortCurrents = new Dictionary<string, Currents>();
        static SMLPowerInfluxDBClient influxClient;
        static PvClient pvClient;
        static IobClient iobClient;
        static SerialPort port;

        internal static int? pvProduction;
        internal static int? chargingPower;

        private static string serialPorts;
        private static string meterIDs;
        private static string influxDb;
        private static string token;
        private static string effectiveBucket;
        private static string cumulativeBucket;
        private static string org;
        private static string pvUrl;
        private static string ioBrokerApiUrl;

        private const int BaudRate = 9600;
        private const Parity PortParity = Parity.None;
        private const int DataBits = 8;



        public static void Main(string[] args)
        {
            if (args.Length != 8)
            {
                Console.WriteLine("Usage: dotnet SMLReader.dll [serialPorts] [meterIDs] [influxDBUrl] [InfluxAuthToken] [influxEffectiveBucket] [influxCumulativeBucket] [influxOrganization] [PvUrl] [IOBrokerSimpleApiUrl]");
                Console.WriteLine("Example: dotnet SMLReader.dll /dev/ttyUSB0,/dev/ttyUSB1 total,heating http://influxdb.fritz.box:8086/ xxxx-xxxxx== myEffectiveBucket myCumulativeBucket myOrg http://pv.fritz.box http://iobroker:8087/");

                return;
            }

            serialPorts = args[0];
            meterIDs = args[1];
            influxDb = args[2];
            token = args[3];
            effectiveBucket = args[4];
            cumulativeBucket = args[5];
            org = args[6];
            pvUrl = args[7];
            ioBrokerApiUrl = args[8];
            string[] portsList = serialPorts.Split(',');
            string[] meterIDsList = meterIDs.Split(',');
            if (meterIDsList.Length != portsList.Length)
            {
                Console.WriteLine("Usage: dotnet SMLReader.dll [serialPorts] [meterIDs] [influxDBUrl] [InfluxAuthToken] [influxEffectiveBucket] [influxCumulativeBucket] [influxOrganization] [PvUrl] [IOBrokerSimpleApiUrl]");
                Console.WriteLine("Error: meterIDs List length must be equal to serialPorts List length.");

                return;
            }

            if (!meterIDsList.Any(s => s=="total"))
            {
                Console.WriteLine("Usage: dotnet SMLReader.dll [serialPorts] [meterIDs] [influxDBUrl] [InfluxAuthToken] [influxEffectiveBucket] [influxCumulativeBucket] [influxOrganization] [PvUrl] [IOBrokerSimpleApiUrl]");
                Console.WriteLine("Error: There must be one meterID with the id 'total'");

                return;
            }
            influxClient = new SMLPowerInfluxDBClient(
             influxDb,
             token,
             effectiveBucket,
             cumulativeBucket,
             org
            );

            pvClient = new PvClient(
             pvUrl
            );

            iobClient = new IobClient(
                ioBrokerApiUrl
            );
            List<SerialPort> ports = new List<SerialPort>();

            for (int i = 0; i < portsList.Length; i++)
            {
                var sPort = portsList[i];
                var sMeterId = meterIDsList[i];
                try
                {

                    port = new SerialPort(sPort, BaudRate, PortParity, DataBits);
                    ports.Add(port);
                    MessagePipe mp = new MessagePipe(sMeterId);
                    mp.DocumentAvailable += MessagePipe_DocumentAvailable;
                    MessagePipes.Add(port.PortName, mp);
                    Currents c = new Currents();
                    PortCurrents.Add(sMeterId, c);
                }
                catch (IOException ex)
                {
                    HandleError(ex, "While instancing the specified port the following Error occured: {0}");
                    influxClient.Dispose();
                    pvClient.Dispose();
                    return;
                }
                port.DataReceived += P_DataReceived;
            }

            Timer persistTimer = new Timer(async (state) =>
            {
                try
                {
                    if (!port.IsOpen)
                    {
                        port.Open();
                    }
                }
                catch (Exception ex)
                {
                    HandleError(ex, "Error occured while trying to open serial port: {0}");
                    Environment.Exit(1);
                }
                    try
                    {
                        pvProduction = pvClient.GetCurrentProduction().Result;

                    }

                    catch (Exception ex)
                    {
                        pvProduction = null;
                        HandleError(ex, "Could not query pv production. Skipping this point: {0}");
                    }
                    try
                    {
                        chargingPower = iobClient.GetCurrentChargingPower().Result;

                    }
                    catch (Exception ex)
                    {
                        chargingPower = null;
                        HandleError(ex, "Could not query charging power. Skipping this point: {0}");
                    }

                await Persist();
            }, null, 0, 10000);

            Timer persistCumulative = new Timer(async (state) =>
            {
                try
                {
                    await PersistCumulative();
                } catch (Exception ex)
                {
                    HandleError(ex, "Could not persist cumulative values: {0}");
                }
            }, null, 5000, 600000);

            Console.CancelKeyPress += (sender, eArgs) =>
            {
                var result = influxClient.PersistEffective().Result;

                quitEvent.Set();
                eArgs.Cancel = true;
                port.Close();
                influxClient.Dispose();
                pvClient.Dispose();
            };
            quitEvent.WaitOne();

        }

        private static async Task<bool> PersistCumulative()
        {
            foreach (string meterId in PortCurrents.Keys)
            {
                if (!PortCurrents[meterId].obis180.HasValue || !PortCurrents[meterId].obis280.HasValue)
                {
                    return false;
                }
            }
            List<IntValue> vals = new List<IntValue>();
            var yield = pvClient.GetTotalYield().Result;
            vals.Add(new IntValue { Name = "yield", Value = yield });
            var charge = iobClient.GetTotalchargingConsumption().Result * 1000;
            vals.Add(new IntValue { Name = "charge", Value = Convert.ToInt32(charge)});
            foreach (string meterId in PortCurrents.Keys)
            {
                if (meterId != "total")
                {
                    vals.Add(new IntValue { Name = $"{meterId}_obis280", Value = PortCurrents[meterId].obis280.Value });
                    vals.Add(new IntValue { Name = $"{meterId}_obis180", Value = PortCurrents[meterId].obis180.Value });
                } else
                {
                    vals.Add(new IntValue { Name = $"obis280", Value = PortCurrents[meterId].obis280.Value });
                    vals.Add(new IntValue { Name = $"obis180", Value = PortCurrents[meterId].obis180.Value });
                }
            }
            var result = await influxClient.PersistCumulative(vals);
            if (!result.IsSuccessMessage)
            {
                throw new SMLException("Influx write Error " + result.ReturnCode + ":" + result.ErrorMessage);
            }
            else
            {
                return true;
            }
        }


        private static void HandleError(Exception Ex, string ExtraInfo)
        {
            Console.WriteLine(String.Format(ExtraInfo, Ex.Message));
        }

        private static async Task Persist()
        {
            if (!pvProduction.HasValue || !chargingPower.HasValue)
            foreach (var meterId in PortCurrents.Keys)
            {
                if (!PortCurrents[meterId].effectivePower.HasValue)
                    return;
            }

            //int effective, int buy, int load, int production, int charge, int load_wo_charge
            var vals = new List<IntValue>();
            var prod = pvProduction.Value;
            vals.Add(new IntValue { Name = "production", Value = prod });
            var pow = PortCurrents["total"].effectivePower.Value;
            vals.Add(new IntValue { Name = "effective", Value = pow });
            var charge = chargingPower.Value;
            vals.Add(new IntValue { Name = "charge", Value = charge });
            var buy = pow > 0 ? pow : 0;
            vals.Add(new IntValue { Name = "buy", Value = buy });
            var load = pow + prod;
            vals.Add(new IntValue { Name = "load", Value = load });
            vals.Add(new IntValue { Name = "load_wo_charge", Value = load - charge });
            var delivery = pow < 0 ? pow*-1 : 0;
            foreach (var meterId in PortCurrents.Keys)
            {
                if (meterId != "total")
                    vals.Add(new IntValue { Name = meterId + "_effective", Value = PortCurrents[meterId].effectivePower.Value });
            }

            influxClient.AddEffectivePoint("instantanious", vals);
            try
            {
                var pdresult = await iobClient.UpdatePowerData(
                    prod,
                    delivery,
                    load,
                    buy);
            } catch (Exception ex)
            {
                HandleError(ex, "Error during persisting to iobroker. Skipping.");
            }
            pvProduction = null;
            PortCurrents["total"].effectivePower = null;

            

            if (!influxClient.QueueClear)
            {
                var result = await influxClient.PersistEffective();
                if (!result.IsSuccessMessage)
                {
                    try
                    {
                        throw new SMLException("Error during persisting to influx. HTTP Status " + result.ReturnCode);
                    }
                    catch (SMLException)
                    {
                        HandleError(new SMLException(result.ErrorMessage), "Error during persisence to influx. HTTP Status " + result.ReturnCode + ": {0}");
                    }
                }
                else
                {
                    //Console.WriteLine($"Persisted instantanious: {pow} effective, {pow+prod} load, {prod} production");
                }
            }
        }

        private static void MessagePipe_DocumentAvailable(object sender, SMLDocumentEventArgs e)
        {
            var meterId = ((MessagePipe)sender).PipeName;
            try
            {
                
                var effectivePowerEntry = ((SMLGetListResponse)e.Document[1].Body).ValList.Where(m => m.ObisCode != null && m.ObisCode.Register == "1-0:16.7.0*255").FirstOrDefault();
                PortCurrents[meterId].effectivePower = (int)effectivePowerEntry.IntValue.Value;

                var obis180Entry = ((SMLGetListResponse)e.Document[1].Body).ValList.Where(m => m.ObisCode != null && m.ObisCode.Register == "1-0:1.8.0*255").FirstOrDefault();
                PortCurrents[meterId].obis180 = (int)obis180Entry.UIntValue.Value / 10;

                var obis280Entry = ((SMLGetListResponse)e.Document[1].Body).ValList.Where(m => m.ObisCode != null && m.ObisCode.Register == "1-0:2.8.0*255").FirstOrDefault();
                PortCurrents[meterId].obis280 = (int)obis280Entry.UIntValue.Value / 10;
                

            }
            catch (Exception ex)
            {
                PortCurrents[meterId].effectivePower = null;
                HandleError(ex, "Could not read effetive Power from SML Message. Skipping this point: {0}");
            }
            try
            {
                port.Close();
            }
            catch (IOException ex)
            {
                HandleError(ex, "Unable to close serial port Port. Anyway continuing. {0}");
            }
            ((MessagePipe)sender).Reset();
            Persist();
        }

        private static void P_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var p = (SerialPort)sender;
            
            byte[] chunk = new byte[p.BytesToRead];
            try
            {
                p.Read(chunk, 0, chunk.Length);
                MessagePipes[p.PortName].AddChunk(chunk);
            }
            catch (Exception Ex)
            {
                HandleError(Ex, "Error during reading bytes from Serial port. Resetting Pipe. {0}");
                MessagePipes[p.PortName].Reset();
            }
        }
    }
}
