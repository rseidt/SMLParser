using NUnit.Framework;
using SMLParser;
using System.Collections.Generic;
using System.IO;

namespace SMLTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestBuild()
        {
            var file = @"SML-Message-20201128-234507.227.bin";
            byte[] SML;
            using (FileStream f = new FileStream(file, FileMode.Open))
            {
                SML = new byte[f.Length];
                f.Read(SML, 0, SML.Length);
            }
            SMLParser.SMLParser p = new SMLParser.SMLParser(SML);
            List<SMLMessage> document = p.Parse();
            SMLBuilder b = new SMLBuilder(document);
            byte[] probe = b.Convert();

            for (int i = 0; i < probe.Length; i++)
            {
                Assert.AreEqual(probe[i], SML[i]);
            }
        }
    }
}