using System;
using System.Net.Security;
using System.Runtime.Loader;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChanceNET;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Newtonsoft.Json;
using Serilog;

namespace DotNetDataGenerator
{
    internal class Program
    {
        private static int _counter;
        private static bool _airconActive = false;
        private static double _tempChange = 1.5d;

        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                //  .WriteTo.Trace()
                .CreateLogger();

            Log.Information("DotNetDataGenerator");
            Log.Information(Environment.GetEnvironmentVariable("IOTEDGE_WORKLOADURI"));
            Log.Information(Environment.GetEnvironmentVariable("IOTEDGE_IOTHUBHOSTNAME"));
            Log.Information(Environment.GetEnvironmentVariable("IOTEDGE_GATEWAYHOSTNAME"));
            Log.Information(Environment.GetEnvironmentVariable("EdgeHubConnectionString"));
            Log.Information(Environment.GetEnvironmentVariable("IotHubConnectionString"));
            Log.Information(Environment.GetEnvironmentVariable("EdgeModuleCACertificateFile"));

            try
            {

                Init().Wait();

                //Wait until the app unloads or is cancelled
                var cts = new CancellationTokenSource();
                AssemblyLoadContext.Default.Unloading += ctx => cts.Cancel();
                Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
                WhenCancelled(cts.Token).Wait();
            }
            catch (Exception ex)
            {
                Log.Warning(ex.Message);
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        ///     Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        ///     Initializes the ModuleClient and sets up the callback to receive
        /// </summary>
        private static async Task Init()
        {
            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient;

            var connectionSettings = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only); // setup connection to hubs MQTT broker
            ITransportSettings[] settings = { connectionSettings };
            ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(); // inbuilt SDK magic to connect using environment variables

            try
            {
                await ioTHubModuleClient.OpenAsync(); // 

                // Create a handler for the direct method calls
                await ioTHubModuleClient.SetMethodHandlerAsync("ActivateAirCon", ActivateAirCon, ioTHubModuleClient);
                await ioTHubModuleClient.SetMethodHandlerAsync("SetTempChange", SetTempChange, ioTHubModuleClient);

                // Register callback to be called when a message is received by the module
                await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", NifiMessageReceived, ioTHubModuleClient);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unable to open connection");
                throw;
            }

            var chance = new Chance(42); // random data generator
            var payload = chance.Object<Payload>();
            double currentTemp = 18d; // start at 18 degrees celcius

            for (int i = 0; i < 1000; i++)
            {
                if (_airconActive)
                {
                    // air con is active - cool down
                    if (currentTemp >= 18)
                    {
                        currentTemp = currentTemp - _tempChange;
                    }
                    else
                    {
                        // fix the tempat 18 if it goes below
                        currentTemp = 18;
                    }

                    Log.Information($"AIR CON ACTIVE. Temp: {currentTemp}");
                }
                else
                {
                    // air con OFF increase the heat 
                    Log.Information($"Increasing heat. Temp: {currentTemp}");
                    currentTemp = currentTemp + chance.Double(1, _tempChange);
                }

                payload.Temperature = currentTemp;
                payload.IsAirConditionerOn = _airconActive;
                payload.TagKey = "dotnet";
                payload.TimeStamp = DateTime.Now; // just display in local time for demo

                var msg = JsonConvert.SerializeObject(payload);
                var messageBytes = Encoding.UTF8.GetBytes(msg);

                try
                {
                    using (var pipeMessage = new Message(messageBytes))
                    {
                        await ioTHubModuleClient.SendEventAsync("output1", pipeMessage);

                        Log.Information("sent: " + msg);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "DotNetDataGenerator {0}", payload);
                }

                Thread.Sleep(TimeSpan.FromSeconds(60));
                i++;
            }
        }

        // Handle the direct method call to turn on air con
        private static Task<MethodResponse> ActivateAirCon(MethodRequest methodRequest, object userContext)
        {

            Log.Information("ActivateAirCon direct method called");
            var data = Encoding.UTF8.GetString(methodRequest.Data);

            // set air con
            _airconActive = Convert.ToBoolean(data);

            string result = "{\"result\":\"Executed direct method ActivateAirCon: " + _airconActive.ToString() + "\"}";
            Log.Information(result);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }


        //_tempChange
        private static Task<MethodResponse> SetTempChange(MethodRequest methodRequest, object userContext)
        {

            Log.Information("SetTempChange direct method called");
            var data = Encoding.UTF8.GetString(methodRequest.Data);

            // set air con
            _tempChange = Convert.ToDouble(data);

            string result = "{\"result\":\"Executed direct method SetTempChange: " + _tempChange.ToString() + "\"}";
            Log.Information(result);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        /// <summary>
        ///  Send payload message to the EdgeHub.
        /// </summary>
        private static async Task<MessageResponse> PublishTemperature(Message message, object userContext)
        {
            var counterValue = Interlocked.Increment(ref _counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");

            var messageBytes = message.GetBytes();
            var messageString = Encoding.UTF8.GetString(messageBytes);
            Log.Information($"Received message: {counterValue}, Body: [{messageString}]");

            if (!string.IsNullOrEmpty(messageString))
                using (var pipeMessage = new Message(messageBytes))
                {
                    foreach (var prop in message.Properties) pipeMessage.Properties.Add(prop.Key, prop.Value);
                    await moduleClient.SendEventAsync("output1", pipeMessage);

                    Console.WriteLine("Received message sent");
                }

            return MessageResponse.Completed;
        }

        /// <summary>
        /// receive msg from Nifi - temp too hot
        /// </summary>
        static async Task<MessageResponse> NifiMessageReceived(Message message, object userContext)
        {
            int counterValue = Interlocked.Increment(ref _counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Payload payload = JsonConvert.DeserializeObject<Payload>(messageString.TrimStart('"').TrimEnd('"').Replace("\\", String.Empty));

            Log.Information($"received:{messageString}");

            if (payload.IsAirConditionerOn == false)
            { // HOT!
                Log.Information("########################################");
                Log.Information($"TURNING ON AIR CON AUTOMATICALLY");
                Log.Information("########################################");
                _airconActive = true;
            }

            return await Task.FromResult(MessageResponse.Completed);
        }


        public static bool ValidateServerCertificate(
                object sender,
                X509Certificate certificate,
                X509Chain chain,
                SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            return true;
        }
    }
}