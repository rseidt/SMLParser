using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SMLParser
{
    public class MessagePipe
    {
        System.IO.MemoryStream SourceStream;
        public event EventHandler<SMLDocumentEventArgs> DocumentAvailable;
        private ulong messageCount = 0;

        public MessagePipe()
        {
            SourceStream = new System.IO.MemoryStream();
        }

        public void AddChunk(byte[] DataChunk)
        {
            SourceStream.Write(DataChunk, 0, DataChunk.Length);
            CheckMessage();
        }

        protected virtual void OnDocumentAvailable(SMLDocumentEventArgs e)
        {
            EventHandler<SMLDocumentEventArgs> handler = DocumentAvailable;
            handler?.Invoke(this, e);
        }

        private void CheckMessage()
        {
            if (SourceStream.Length < 16)
                return;
            var currentChunk = SourceStream.ToArray();
            int[] beginindicatorLocations = currentChunk.Locate(SMLParser.beginIndicator);
            int[] endindicatorLocations = currentChunk.Locate(SMLParser.endIndicator);
            while (beginindicatorLocations.Length > 0 && endindicatorLocations.Length > 0 && endindicatorLocations.Any(ei => ei > beginindicatorLocations[0] && ei < SourceStream.Length - 8))
            {
                SMLParser p = new SMLParser(SourceStream.ToArray());
                int LastMessageIndex = endindicatorLocations.First(ei => ei > beginindicatorLocations[0] && ei < SourceStream.Length - 8) + 7;
                if (SourceStream.Length > LastMessageIndex + 1)
                {
                    SourceStream.Seek(LastMessageIndex + 1, System.IO.SeekOrigin.Begin);
                    byte[] overhangbytes = new byte[SourceStream.Length - LastMessageIndex - 1];
                    SourceStream.Read(overhangbytes, 0, overhangbytes.Length);
                    SourceStream.Position = 0;
                    SourceStream.SetLength(0);
                    SourceStream.Write(overhangbytes, 0, overhangbytes.Length);
                }
                else
                {
                    SourceStream.Position = 0;
                    SourceStream.SetLength(0);
                }
                var result = p.Parse();
                messageCount++;
                beginindicatorLocations = currentChunk.Locate(SMLParser.beginIndicator);
                endindicatorLocations = currentChunk.Locate(SMLParser.endIndicator);
                OnDocumentAvailable(new SMLDocumentEventArgs { Document = result });
            }

        }
            public static void PrintValue(RawRecord v, int indent)
            {
                if (v.Type == RawType.List)
                {
                    Console.Write("-".PadLeft(indent * 2 + 1));
                    Console.Write("Liste mit " + v.Length + " Einträgen:\r\n");
                    foreach (RawRecord subval in ((RawSequence)v).Items)
                    {
                        PrintValue(subval, indent + 1);
                    }
                }
                else
                {
                    Console.Write("-".PadLeft(indent * 2 + 1));
                    Console.Write("Len: " + v.Length + "; Type: " + v.Type + ", Value: ");
                    if (((SMLValue)v).Value == null)
                    {
                        Console.Write("<NULL>");
                    }
                    else
                    {
                        foreach (byte b in ((SMLValue)v).Value)
                        {
                            Console.Write(b.ToString("x").PadLeft(2, '0') + " ");
                        }
                    }
                    Console.Write("\r\n");
                }
            }
        }
    }
