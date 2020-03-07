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
            string connection = Environment.GetEnvironmentVariable("EdgeHubConnectionString");
            if (string.IsNullOrEmpty(connection))
            {
                connection = Environment.GetEnvironmentVariable("EdgeHubConnectionString", EnvironmentVariableTarget.User);
            }
            try
            {
                if (string.IsNullOrEmpty(connection))
                {
                    connection = Environment.GetEnvironmentVariable("EdgeHubConnectionString", EnvironmentVariableTarget.Machine);
                }
            }
            catch { }


            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient;
            //if (!string.IsNullOrEmpty(connection))
            var connectionSettings = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
                //connectionSettings.CleanSession = true;
                //connectionSettings.ValidateServerCertificate = ValidateServerCertificate;
                connectionSettings.RemoteCertificateValidationCallback = ValidateServerCertificate;
                ITransportSettings[] settings = { connectionSettings };
                Log.Information($"CreateFromEnvironmentAsync");
                ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync();

            try{
                await ioTHubModuleClient.OpenAsync();
            }
            catch(Exception ex){
                Log.Error(ex, "await ioTHubModuleClient.OpenAsync();");
                throw;
            }

            // send random data
            var chance = new Chance(42);

            var payload = chance.Object<Payload>();

            for (int i = 0; i < 1000; i++)
            {
                payload.Temperature = chance.Double(0, 1);
                payload.IsAirConditionerOn = chance.Bool(payload.Temperature);
                payload.TagKey = "dotnet";
                payload.TimeStamp = DateTime.UtcNow;
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

                Thread.Sleep(10000);
                i++;
            }
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

        /// <summary>
        ///     This method is called whenever the module is sent a message from the EdgeHub.
        ///     It just pipe the messages without any change.
        ///     It prints all the incoming messages.
        /// </summary>
        private static async Task<MessageResponse> PipeMessage(Message message, object userContext)
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
    }
}