using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using SMLParser;
using System.Threading.Tasks;

namespace SMLReader
{
    class SMLReder
    {

        static ManualResetEvent quitEvent = new ManualResetEvent(false);

        static MessagePipe messagePipe = new MessagePipe();

        static SMLPowerInfluxDBClient influxClient;
        static PvClient pvClient;
        static ChargerClient chargerClient;
        static SerialPort port;


        private static string serialPort;
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

        private static int? effectivePower;
        private static int? pvProduction;

        private static int? chargingPower;

        private static int? obis180;
        private static int? obis280;

        static void Main(string[] args)
        {
            if (args.Length != 8)
            {
                Console.WriteLine("Usage: dotnet SMLReader.dll [serialPort] [influxDBUrl] [InfluxAuthToken] [influxEffectiveBucket] [influxCumulativeBucket] [influxOrganization] [PvUrl] [IOBrokerSimpleApiUrl]");
                Console.WriteLine("Example: dotnet SMLReader.dll /dev/ttyUSB0 http://influxdb.fritz.box:8086/ xxxx-xxxxx== myEffectiveBucket myCumulativeBucket myOrg http://pv.fritz.box http://iobroker:8087/");

                return;
            }

            serialPort = args[0];
            influxDb = args[1];
            token = args[2];
            effectiveBucket = args[3];
            cumulativeBucket = args[4];
            org = args[5];
            pvUrl = args[6];
            ioBrokerApiUrl = args[7];

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

            chargerClient = new ChargerClient(
                ioBrokerApiUrl
            );

            messagePipe.DocumentAvailable += MessagePipe_DocumentAvailable;
            try
            {
                port = new SerialPort(serialPort, BaudRate, PortParity, DataBits);
            }
            catch (IOException ex)
            {
                HandleError(ex, "While instancing the specified port the following Error occured: {0}");
                influxClient.Dispose();
                pvClient.Dispose();
                return;
            }
            port.DataReceived += P_DataReceived;

            Timer persistTimer = new Timer((state) =>
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
                    chargingPower = chargerClient.GetCurrentChargingPower().Result;

                }
                catch (Exception ex)
                {
                    chargingPower = null;
                    HandleError(ex, "Could not query charging power. Skipping this point: {0}");
                }
                Persist();
            }, null, 0, 10000);

            Timer persistCumulative = new Timer((state) =>
            {
                try
                {
                    PersistCumulative();
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

        private static bool PersistCumulative()
        {
            if (!obis180.HasValue || !obis280.HasValue)
            {
                return false;
            }
            var yield = pvClient.GetTotalYield().Result;
            var charge = chargerClient.GetTotalchargingConsumption().Result;
            var result = influxClient.PersistCumulative(obis280.Value, obis180.Value, yield, (int)charge).Result;
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

        private static void Persist()
        {
            if (!effectivePower.HasValue || !pvProduction.HasValue)
                return;
            var prod = pvProduction.Value;
            var pow = effectivePower.Value;
            var charge = chargingPower.Value;
            influxClient.AddEffectivePoint("instantanious", pow, pow > 0 ? pow : 0, pow + prod, prod, charge, pow + prod - charge);
            pvProduction = null;
            effectivePower = null;


            if (!influxClient.QueueClear)
            {
                var result = influxClient.PersistEffective().Result;
                if (!result.IsSuccessMessage)
                {
                    try
                    {
                        throw new SMLException("Error during persisence to influx. HTTP Status " + result.ReturnCode);
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
            try
            {
                var effectivePowerEntry = ((SMLGetListResponse)e.Document[1].Body).ValList.Where(m => m.ObisCode != null && m.ObisCode.Register == "1-0:16.7.0*255").FirstOrDefault();
                effectivePower = (int)effectivePowerEntry.IntValue.Value;

                var obis180Entry = ((SMLGetListResponse)e.Document[1].Body).ValList.Where(m => m.ObisCode != null && m.ObisCode.Register == "1-0:1.8.0*255").FirstOrDefault();
                obis180 = (int)obis180Entry.UIntValue.Value / 10;

                var obis280Entry = ((SMLGetListResponse)e.Document[1].Body).ValList.Where(m => m.ObisCode != null && m.ObisCode.Register == "1-0:2.8.0*255").FirstOrDefault();
                obis280 = (int)obis280Entry.UIntValue.Value / 10;

            }
            catch (Exception ex)
            {
                effectivePower = null;
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
            messagePipe.Reset();
            Persist();
        }

        private static void P_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var p = (SerialPort)sender;
            byte[] chunk = new byte[p.BytesToRead];
            try
            {
                p.Read(chunk, 0, chunk.Length);
                messagePipe.AddChunk(chunk);
            }
            catch (Exception Ex)
            {
                HandleError(Ex, "Error during reading bytes from Serial port. Resetting Pipe. {0}");
                messagePipe.Reset();
            }
        }
    }
}
