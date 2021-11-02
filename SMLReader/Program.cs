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



    class SMLReader
    {
        static ManualResetEvent quitEvent = new ManualResetEvent(false);

        static readonly Dictionary<string, MessagePipe> MessagePipes = new Dictionary<string, MessagePipe>();
        static readonly Dictionary<string, Currents> PortCurrents = new Dictionary<string, Currents>();
        static SMLPowerInfluxDBClient influxClient;
        static PvClient pvClient;
        static IobClient iobClient;

        private static bool debug = false;

        private static List<SerialPort> ports = new List<SerialPort>();

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
            if (args.Length != 9 && args.Length != 10)
            {
                Console.WriteLine("Usage: dotnet SMLReader.dll [serialPorts] [meterIDs] [influxDBUrl] [InfluxAuthToken] [influxEffectiveBucket] [influxCumulativeBucket] [influxOrganization] [PvUrl] [IOBrokerSimpleApiUrl] [(optional):'debug']");
                Console.WriteLine("Example: dotnet SMLReader.dll /dev/ttyUSB0,/dev/ttyUSB1 total,heating http://influxdb.fritz.box:8086/ xxxx-xxxxx== myEffectiveBucket myCumulativeBucket myOrg http://pv.fritz.box http://iobroker:8087/ debug");

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
            if (args.Length == 10 && args[9] == "debug")
            {
                debug = true;
            }
            string[] portsList = serialPorts.Split(',');
            string[] meterIDsList = meterIDs.Split(',');
            if (meterIDsList.Length != portsList.Length)
            {
                Console.WriteLine("Usage: dotnet SMLReader.dll [serialPorts] [meterIDs] [influxDBUrl] [InfluxAuthToken] [influxEffectiveBucket] [influxCumulativeBucket] [influxOrganization] [PvUrl] [IOBrokerSimpleApiUrl] [(optional):'debug']");
                Console.WriteLine("Error: meterIDs List length must be equal to serialPorts List length.");

                return;
            }

            if (!meterIDsList.Any(s => s=="total"))
            {
                Console.WriteLine("Usage: dotnet SMLReader.dll [serialPorts] [meterIDs] [influxDBUrl] [InfluxAuthToken] [influxEffectiveBucket] [influxCumulativeBucket] [influxOrganization] [PvUrl] [IOBrokerSimpleApiUrl] [(optional):'debug']");
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
            

            for (int i = 0; i < portsList.Length; i++)
            {
                var sPort = portsList[i];
                var sMeterId = meterIDsList[i];
                try
                {

                    var port = new SerialPort(sPort, BaudRate, PortParity, DataBits);
                    ports.Add(port);
                    MessagePipe mp = new MessagePipe(sMeterId);
                    mp.DocumentAvailable += MessagePipe_DocumentAvailable;
                    MessagePipes.Add(port.PortName, mp);
                    Currents c = new Currents();
                    PortCurrents.Add(sMeterId, c);
                    port.DataReceived += P_DataReceived;
                    port.Open();
                }
                catch (IOException ex)
                {
                    HandleError(ex, "While instancing the specified port the following Error occured: {0}");
                    influxClient.Dispose();
                    pvClient.Dispose();
                    return;
                }
                
            }

            System.Timers.Timer persistEffectiveTimer = new System.Timers.Timer(10000);
            persistEffectiveTimer.Elapsed += PersistEffectiveTimer_Elapsed;

            System.Timers.Timer persistCoumulativeTimer = new System.Timers.Timer(60000);
            persistCoumulativeTimer.Elapsed += PersistCoumulativeTimer_Elapsed;

            Console.CancelKeyPress += (sender, eArgs) =>
            {
                var result = influxClient.PersistEffective().Result;

                quitEvent.Set();
                eArgs.Cancel = true;
                foreach (var port in ports)
                {
                    port.Close();
                }
                influxClient.Dispose();
                pvClient.Dispose();
            };
            
            quitEvent.WaitOne();

        }

        private static async void PersistEffectiveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (debug)
                Console.WriteLine("Starting instantanious timer");
            try
            {
                SMLReader.pvProduction = pvClient.GetCurrentProduction().Result;
                if (debug)
                    Console.WriteLine("Received '" + pvProduction.Value.ToString() + "' as PV production");

            }
            catch (Exception ex)
            {
                SMLReader.pvProduction = null;
                HandleError(ex, "Could not query pv production. Skipping this point: {0}");
            }
            try
            {
                SMLReader.chargingPower = iobClient.GetCurrentChargingPower().Result;
                if (debug)
                    Console.WriteLine("Received '" + chargingPower.Value.ToString() + "' as Charging power");
            }
            catch (Exception ex)
            {
                SMLReader.chargingPower = null;
                HandleError(ex, "Could not query charging power. Skipping this point: {0}");
            }
            try
            {
                await Persist();
            }
            catch (Exception ex)
            {
                HandleError(ex, "Could not persist effective values: {0}");
            }
        }

        private static async void PersistCoumulativeTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (debug)
                Console.WriteLine("Start cumulative timer");
            try
            {
                await PersistCumulative();
            }
            catch (Exception ex)
            {
                HandleError(ex, "Could not persist cumulative values: {0}");
            }

        }

        private static async Task<bool> PersistCumulative()
        {
            foreach (string meterId in PortCurrents.Keys)
            {
                if (!PortCurrents[meterId].obis180.HasValue || !PortCurrents[meterId].obis280.HasValue)
                {
                    if (debug)
                        Console.WriteLine("no cumulative Values in meterID '" + meterId + "'");
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
                if (debug)
                {
                    Console.WriteLine("Persisted cumulative values:");
                    foreach (var val in vals)
                    {
                        Console.WriteLine("\t" + val.Name + ": " + val.Value);
                    }
                }
                return true;
            }
        }


        private static void HandleError(Exception Ex, string ExtraInfo)
        {
            Console.WriteLine(String.Format(ExtraInfo, Ex.Message));
        }

        private static async Task Persist()
        {
            if (!SMLReader.pvProduction.HasValue || !SMLReader.chargingPower.HasValue)
            {
                if (debug)
                    Console.WriteLine("nothing effective to persist in pvProduction or charging Power");
                return;
            }
            foreach (var meterId in PortCurrents.Keys)
            {
                if (!PortCurrents[meterId].effectivePower.HasValue)
                {
                    if (debug)
                        Console.WriteLine("nothing effective to persist in meterId '"+meterId+"'");
                    return;
                }
            }

            if (debug)
                Console.WriteLine("Values available. Persisting...");

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

            if (debug)
            {
                Console.WriteLine("Collected the following values:");
                foreach (var val in vals)
                {
                    Console.WriteLine("Name: " + val.Name + ", Value: " + val.Value);
                }
            }

            var sb = influxClient.AddEffectivePoint("instantanious", vals);
            if (debug)
                Console.WriteLine(sb.ToString());
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
                    if (debug)
                        Console.WriteLine($"Persisted instantanious: {pow} effective, {pow+prod} load, {prod} production");
                }
            }
        }

        private static void MessagePipe_DocumentAvailable(object sender, SMLDocumentEventArgs e)
        {
            var meterId = ((MessagePipe)sender).PipeName;
            if (debug)
                Console.WriteLine("Received document in pipe '" + meterId + "'");
            try
            {
                
                var effectivePowerEntry = ((SMLGetListResponse)e.Document[1].Body).ValList.Where(m => m.ObisCode != null && m.ObisCode.Register == "1-0:16.7.0*255").FirstOrDefault();
                PortCurrents[meterId].effectivePower = (int)effectivePowerEntry.IntValue.Value;

                var obis180Entry = ((SMLGetListResponse)e.Document[1].Body).ValList.Where(m => m.ObisCode != null && m.ObisCode.Register == "1-0:1.8.0*255").FirstOrDefault();
                PortCurrents[meterId].obis180 = (int)obis180Entry.UIntValue.Value / 10;

                var obis280Entry = ((SMLGetListResponse)e.Document[1].Body).ValList.Where(m => m.ObisCode != null && m.ObisCode.Register == "1-0:2.8.0*255").FirstOrDefault();
                PortCurrents[meterId].obis280 = (int)obis280Entry.UIntValue.Value / 10;
                if (debug)
                    Console.WriteLine("Pipe '" + meterId + "' document: effective:" + PortCurrents[meterId].effectivePower.ToString() +", obis180: " + PortCurrents[meterId].obis180.ToString() + ", obis280: " + PortCurrents[meterId].obis280);

            }
            catch (Exception ex)
            {
                PortCurrents[meterId].effectivePower = null;
                HandleError(ex, "Could not read effetive Power from SML Message. Skipping this point: {0}");
            }
            ((MessagePipe)sender).Reset();
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
