using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMLReader
{
    public delegate Task Callback(string message);

    public class MqttClient : IAsyncDisposable, IDisposable
    {
        private IMqttClient _client = null;
        private Callback _callback = null; 
        public async Task Connect(Callback callback)
        {
            var mqttFactory = new MqttFactory();

            _client = mqttFactory.CreateMqttClient();

            _callback = callback;

            // Use builder classes where possible in this project.
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("iobroker.fritz.box").WithCleanSession().Build();

            // This will throw an exception if the server is not available.
            // The result from this message returns additional data which was sent 
            // from the server. Please refer to the MQTT protocol specification for details.

            _ = Task.Run(
                async () =>
                {
                        // User proper cancellation and no while(true).
                        while (true)
                    {
                        try
                        {
                                // This code will also do the very first connect! So no call to _ConnectAsync_ is required in the first place.
                                if (!_client.IsConnected)
                            {
                                var response = await _client.ConnectAsync(mqttClientOptions, CancellationToken.None);

                                    // Subscribe to topics when session is clean etc.
                                    Console.WriteLine("The MQTT client is connected.");
                                await Subscribe();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine("Unable to Connect: " + ex.Message);
                        }
                        finally
                        {
                            await Task.Delay(TimeSpan.FromSeconds(5));
                        }
                    }
                });

        }

        private async Task Subscribe()
        {
            var mqttFactory = new MqttFactory();

            // Setup message handling before connecting so that queued messages
            // are also handled properly. When there is no event handler attached all
            // received messages get lost.
            _client.ApplicationMessageReceivedAsync += async (e) =>
            {
                //Console.WriteLine("Received application message.");
                string Message = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                await _callback(Message);
            };

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => { f.WithTopic("energy/growatt"); })
                .Build();

            await _client.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            Console.WriteLine("MQTT client subscribed to topic.");
        }

        private async Task Unsubscribe()
        {
            var mqttFactory = new MqttFactory();


            var mqttSubscribeOptions = mqttFactory.CreateUnsubscribeOptionsBuilder()
                .WithTopicFilter("energy/growatt")
                .Build();

            await _client.UnsubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            Console.WriteLine("MQTT client unsubscribed to topic.");


        }

        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public async ValueTask DisposeAsync()
        {
            if (_client != null)
            {
                if (_client.IsConnected)
                {
                    await this.Unsubscribe();
                    var mqttFactory = new MqttFactory();

                    // Send a clean disconnect to the server by calling _DisconnectAsync_. Without this the TCP connection
                    // gets dropped and the server will handle this as a non clean disconnect (see MQTT spec for details).
                    var mqttClientDisconnectOptions = mqttFactory.CreateClientDisconnectOptionsBuilder().Build();

                    await _client.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
                }
                _client.Dispose();
            }
        }
    }
}

