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

        public static SMLPowerInfluxDBClient client; 

        //static void Main(string[] args)
        //{
        //    client.AddPoint("effp", "watts", DateTime.Now.Millisecond);
        //    var result = client.Persist().Result;
        //    Console.WriteLine("Result:" + result.ErrorMessage);
        //}


        private static string serialPort;
        private static string influxDb;
        private static string token;
        private static string bucket;
        private static string org;


        static void Main(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine("Usage: dotnet SMLReader.dll [serialPort] [influxDBUrl] [InfluxAuthToken] [influxBucket] [influxOrganization]");
                Console.WriteLine("Example: dotnet SMLReader.dll /dev/ttyUSB0 http://influxdb.fritz.box:8086/ xxxx-xxxxx== myBucket myOrg");
                return;
            }

            serialPort = args[0];
            influxDb = args[1];
            token = args[2];
            bucket = args[3];
            org = args[4];


            client = new SMLPowerInfluxDBClient(
             influxDb,
             token,
             bucket,
             org
            );

            int baudRate = 9600;

            SerialPort p = new SerialPort(serialPort, baudRate, Parity.None, 8);

            Timer persistTimer = new Timer((state) => {
                if (!client.QueueClear)
                {
                    var result = client.Persist().Result;
                    if (!result.IsSuccessMessage)
                    {
                        Console.WriteLine("ERROR: " + result.ErrorMessage);
                    } else
                    {
                        Console.WriteLine("Persisted documents");
                    }
                }
            }, null, 5000, 5000);

            messagePipe.DocumentAvailable += MessagePipe_DocumentAvailable;
            p.Open();
            p.DataReceived += P_DataReceived;

            Console.CancelKeyPress += (sender, eArgs) =>
            {
                var result = client.Persist().Result;

                quitEvent.Set();
                eArgs.Cancel = true;
                p.Close();
            };
            quitEvent.WaitOne();

        }

        private static void MessagePipe_DocumentAvailable(object sender, SMLDocumentEventArgs e)
        {
            var effectivePowerEntry = ((SMLGetListResponse)e.Document[1].Body).ValList.Where(m => m.ObisCode != null && m.ObisCode.Register == "1-0:16.7.0*255").FirstOrDefault();

            client.AddPoint("effp", "watts", (int)effectivePowerEntry.IntValue.Value);
            Console.WriteLine("Current Effective Power: " + (int)effectivePowerEntry.IntValue.Value + " W");
        }

        private static void P_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var p = (SerialPort)sender;
            byte[] chunk = new byte[p.BytesToRead];
            p.Read(chunk, 0, chunk.Length);
            messagePipe.AddChunk(chunk);
        }
    }
}
