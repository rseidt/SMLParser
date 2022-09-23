using System;
using System.Collections.Generic;
using System.Text;
using SMLReader;
using NUnit.Framework;
using System.Threading.Tasks;

namespace SMLTests
{
    class MqttTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task TestMqtt()
        {
            using (var mqttClient = new MqttClient())
            {
                bool breaker = false;
                await mqttClient.Connect(async (string message) =>
                {
                    int pvProduction2;
                    int yield2;
                    GrowattStatus status = Newtonsoft.Json.JsonConvert.DeserializeObject<GrowattStatus>(message);
                    if (status.InverterStatus == -1)
                    {
                        pvProduction2 = 0;
                        yield2 = 0;
                    }
                    else
                    {
                        pvProduction2 = Convert.ToInt32(status.OutputPower);
                        yield2 = Convert.ToInt32(status.PV1EnergyTotal);
                    }
                    Console.WriteLine("Yield: " + yield2.ToString());
                    Console.WriteLine("Production: " + pvProduction2.ToString());
                });
                while (!breaker)
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}
