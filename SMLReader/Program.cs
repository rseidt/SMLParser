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
        static SerialPort port;


        private static string serialPort;
        private static string influxDb;
        private static string token;
        private static string bucket;
        private static string org;
        private static string pvUrl;


        private const int BaudRate = 9600;
        private const Parity PortParity = Parity.None;
        private const int DataBits = 8;

        private static int? effectivePower;
        private static int? pvProduction;

        static void Main(string[] args)
        {
            if (args.Length != 6)
            {
                Console.WriteLine("Usage: dotnet SMLReader.dll [serialPort] [influxDBUrl] [InfluxAuthToken] [influxBucket] [influxOrganization] [PvUrl]");
                Console.WriteLine("Example: dotnet SMLReader.dll /dev/ttyUSB0 http://influxdb.fritz.box:8086/ xxxx-xxxxx== myBucket myOrg http://pv.fritz.box");

                //Console.WriteLine("Expecting 6 Parameters, but found " + args.Length);
                //foreach (string arg in args)
                //{
                //    Console.WriteLine("- " + arg);
                //}

                return;
            }

            serialPort = args[0];
            influxDb = args[1];
            token = args[2];
            bucket = args[3];
            org = args[4];
            pvUrl = args[5];


            influxClient = new SMLPowerInfluxDBClient(
             influxDb,
             token,
             bucket,
             org
            );

            pvClient = new PvClient(
             pvUrl
            );

            messagePipe.DocumentAvailable += MessagePipe_DocumentAvailable;
            try
            {
                port = new SerialPort(serialPort, BaudRate, PortParity, DataBits);
            } catch (IOException ex)
            {
                HandleError(ex, "While instancing the specified port the following Error occured: {0}");
                influxClient.Dispose();
                pvClient.Dispose();
                return;
            }
            port.DataReceived += P_DataReceived;

            Timer persistTimer = new Timer((state) => {
                try
                {
                    if (!port.IsOpen)
                    {
                        port.Open();
                    }
                } catch (Exception ex)
                {
                    HandleError(ex, "Error occured while trying to open serial port: {0}");
                    Environment.Exit(1);
                }
                try
                {
                    pvProduction = pvClient.GetCurrentProduction().Result;

                } catch (Exception ex)
                {
                    pvProduction = null;
                    HandleError(ex, "Could not query pv production. Skipping this point: {0}");
                }
                Persist();

            }, null, 0, 10000);



            Console.CancelKeyPress += (sender, eArgs) =>
            {
                var result = influxClient.Persist().Result;

                quitEvent.Set();
                eArgs.Cancel = true;
                port.Close();
                influxClient.Dispose();
                pvClient.Dispose();
            };
            quitEvent.WaitOne();

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
            influxClient.AddPoint("instantanious", pow, pow + prod, prod);
            pvProduction = null;
            effectivePower = null;


            if (!influxClient.QueueClear)
            {
                var result = influxClient.Persist().Result;
                if (!result.IsSuccessMessage)
                {
                    try
                    {
                        throw new SMLException("Error during persisence to influx. HTTP Status " + result.ReturnCode);
                    } catch(SMLException)
                    {
                        HandleError(new SMLException(result.ErrorMessage), "Error during persisence to influx. HTTP Status " + result.ReturnCode + ": {0}");
                    }
                }
                else
                {
                    Console.WriteLine($"Persisted instantanious: {pow} effective, {pow+prod} load, {prod} production");
                }
            }
        }

        private static void MessagePipe_DocumentAvailable(object sender, SMLDocumentEventArgs e)
        {
            try
            {
                var effectivePowerEntry = ((SMLGetListResponse)e.Document[1].Body).ValList.Where(m => m.ObisCode != null && m.ObisCode.Register == "1-0:16.7.0*255").FirstOrDefault();
                effectivePower = (int)effectivePowerEntry.IntValue.Value;
            } catch(Exception ex)
            {
                effectivePower = null;
                HandleError(ex, "Could not read effetive Power from SML Message. Skipping this point: {0}");
            }
            try
            {
                port.Close();
            } catch (IOException ex)
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
