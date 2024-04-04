# SML Parser

This project implements the most common operations of the SML specification in. There are plenty of implementations, but I did not find one in c#, so I created one.

See details for the specification here: [Spec](https://www.bsi.bund.de/SharedDocs/Downloads/DE/BSI/Publikationen/TechnischeRichtlinien/TR03109/TR-03109-1_Anlage_Feinspezifikation_Drahtgebundene_LMN-Schnittstelle_Teilb.pdf?__blob=publicationFile)

A list of implementstions in other languages is available on the Wikipedia page: https://de.wikipedia.org/wiki/Smart_Message_Language

The specification describes the communication with smart meters in order do receive values of counters as well as current load.

It is designed to work with a continous binary steam as well as just parsing a byte array.

Usually the smart meter is connected using an optical interface which transforms infra red signals into a serial byte stream that is read for example using an USB/serial converter.

## Usage example for reading a byte array:

```CSharp
var file = @"SML-Message-20201128-234507.227.bin";
byte[] SML;
using (var f = new FileStream(file, FileMode.Open))
{
    SML = new byte[f.Length];
    f.Read(SML, 0, SML.Length);
}
var p = new SMLParser.SMLParser(SML);
List<SMLMessage> document = p.Parse();

```

## Usage example for stream reading:
Given that you have connected the Smart meter to the Serial Port COM1, you can define an event handler whenever a complete SML message was received:

```CSharp
private const int BaudRate = 9600;
private const Parity PortParity = Parity.None;
private const int DataBits = 8;
public static void Main(string[] args)
{

    try
    {
        var port = new SerialPort("COM1", BaudRate, PortParity, DataBits);
        var mp = new MessagePipe("Home");
        mp.DocumentAvailable += MessagePipe_DocumentAvailable;
        MessagePipes.Add(port.PortName, mp);
        port.DataReceived += P_DataReceived;
        port.Open();
    }
    catch (IOException ex)
    {
        Console.Writeline($"While instancing the specified port the following Error occured: {ex.Message}");
        return;
    }
}

private static void MessagePipe_DocumentAvailable(object sender, SMLDocumentEventArgs e)
{
    var meterId = ((MessagePipe)sender).PipeName;
    Console.WriteLine("Received document in pipe '" + meterId + "'");
    var obis180Entry = ((SMLGetListResponse)e.Document[1].Body).ValList.Where(m => m.ObisCode != null && m.ObisCode.Register == "1-0:1.8.0*255").FirstOrDefault();
    Console.WriteLine($"Purchased energy total: {obis180Entry.UIntValue.Value / 10}");
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
        Console.Writeline("Error during reading bytes from Serial port. Resetting Pipe.");
        MessagePipes[p.PortName].Reset();
    }
}
```

The smart meter will continously send bytes which form a series of SML documents. The bytes are collected using the `P_DataReceived` event handler (This is part of the SerialPort Class of .Net). The chunk of bytes is handed over to the `MessagePipe` class which identifies the boundaries of SML Messages and will fire the `MessageAvailable` whenever a complete message is available, and returns an object which contains all the register values.
A List of common registers can be found by loking into the `ObisCodes.cs` file.

The implementation of the spec is not complete (See th RawConvert.cs file for missing parsers) but it is good enough to read from a smart meter even with the extended information activated.
