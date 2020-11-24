using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using SMLParser;

namespace SMLReader
{
    class SMLReder
    {

        static ManualResetEvent quitEvent = new ManualResetEvent(false);

        static MessagePipe messagePipe = new MessagePipe();

        static void Main(string[] args)
        {
            int baudRate = 9600;
            var serialPort = "COM5";

            SerialPort p = new SerialPort(serialPort, baudRate, Parity.None, 8);

            messagePipe.DocumentAvailable += MessagePipe_DocumentAvailable;
            p.Open();
            p.DataReceived += P_DataReceived;

            Console.CancelKeyPress += (sender, eArgs) =>
            {
                quitEvent.Set();
                eArgs.Cancel = true;
                p.Close();
            };
            quitEvent.WaitOne();

        }

        private static void MessagePipe_DocumentAvailable(object sender, SMLDocumentEventArgs e)
        {
            var json = JsonConvert.SerializeObject(e.Document);
            Console.Write(json);
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
