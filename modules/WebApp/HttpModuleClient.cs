/*
This is the code that connects to the IoT Edge Hub and receives the shelf data.

The local code to this for testing is the InProcShelfDataGenerator which is used to generate data
without connecting to IoT Edge Hub (primarily for testing purposes).

Set the environment variable in deployment.template.json or launch.json as described in README.md 
to toggle between the two. For the demo, this module is not used.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ChanceNET;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Extensions.Logging;
using Serilog;

namespace WebApp {

/*Create the module as an ASP.NET Core Background Service
Details of this implementation pattern (Queued background tasks section) are available here: 
https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.1#queued-background-tasks
*/
    public class HttpModuleClient : BackgroundService
    {           
        readonly ILogger<HttpModuleClient> _logger;

        private IBackgroundPayloadQueue PayloadQueue {get; set;}
        private ModuleClient ModuleClient {get; set;}

        public HttpModuleClient(IBackgroundPayloadQueue payloadQueue, ILogger<HttpModuleClient> logger)
        { 
            this.PayloadQueue = payloadQueue;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /*The PipeMessage method is what will connect to the IoT Edge Hub and will queue the shelf data
        for processing. 
        
        It will be registered as an inputMessageHandler with the IoT Edge Hub in the ExecuteAsync method
        that follows.
        */
        public async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            /*This section receives the data from the IoT Edge Hub and converts it to a Shelf object 
              for processing through signalR to the WebApp module.
            */
            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);

            if (!string.IsNullOrEmpty(messageString))
            {
                try{
                    _logger.LogInformation($"Receiving Message: {messageString.TrimStart('"').TrimEnd('"').Replace('\\', ' ')}");
       
                    Payload productData = JsonConvert.DeserializeObject<Payload>
                        (messageString.TrimStart('"').TrimEnd('"').Replace("\\",String.Empty));

                    PayloadQueue.QueuePayload(productData);

                    if(PayloadQueue.Count() > 5){
                        while(PayloadQueue.Count() > 5){
                            await PayloadQueue.DequeueAsync(new CancellationToken()); //throw away result
                            _logger.LogInformation("Dequeue extra data");
                        }
                    }
                     
                    // catch and swallow exceptions 
                } catch (AggregateException ex){
                    _logger.LogError($"Error processing message: {ex.Flatten()}");
                } catch (Exception ex){
                    _logger.LogError($"Error processing message: {ex}");
                }
            }
            return MessageResponse.Completed;
        }

        // ExecuteAsync is called when IHostedService starts.  
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
          
            if (!string.IsNullOrEmpty(connection))
            {
                ModuleClient = ModuleClient.CreateFromConnectionString(connection);
            }
            else
            {
                var connectionSettings = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
                //settings.CleanSession = true;
                connectionSettings.RemoteCertificateValidationCallback = ValidateServerCertificate;
                ITransportSettings[] settings = { connectionSettings };
                ModuleClient = await ModuleClient.CreateFromEnvironmentAsync();
            }
            await ModuleClient.OpenAsync(stoppingToken);
            await ModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ModuleClient, stoppingToken);
            Log.Information("input1 listener setup ok");


           // await GenerateRandomData();
        }

        private static async Task GenerateRandomData()
        {
            var mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            mqttSetting.CleanSession = true;
            mqttSetting.RemoteCertificateValidationCallback = ValidateServerCertificate;
            ITransportSettings[] settings = {mqttSetting};

            // Open a connection to the Edge runtime
            var ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Log.Information("IoT Hub module client initialized.");

            // send random data
            var chance = new Chance(42);

            var payload = chance.Object<Payload>();

            //for (int i = 0; i < 1000; i++)
            var i = 1;
            while (i < 500)
            {
                payload.Temperature = chance.Double(0, 1);
                payload.IsAlive = chance.Bool(payload.Temperature);
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
                    Log.Error(ex, "WebApp GenerateRandomData {0}", payload);
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
    }



    /* Create the interfaces for the singleton dependency injection that are used
    to coordinate receiving shelves that are received over IoT Edge Hub and sent to
    the HMI
    */
    public interface IBackgroundPayloadQueue
    {
        void QueuePayload(Payload workItem);

        Task<Payload> DequeueAsync(CancellationToken token);

        int Count();

        IEnumerable<Payload> List();
    }

    /* Create the implementations for the interfaces above */

    public class BackgroundPayloadQueue : IBackgroundPayloadQueue
    {
        private ConcurrentQueue<Payload> _payloads = new ConcurrentQueue<Payload>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void QueuePayload(Payload shelf)
        {
            if (shelf == null)
            {
                throw new ArgumentNullException(nameof(shelf));
            }

            _payloads.Enqueue(shelf);
            _signal.Release();
        }

        public async Task<Payload> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _payloads.TryDequeue(out var payload);

            return payload;
        }

        public int Count()
        {
            return _payloads.Count();
        }

        public IEnumerable<Payload> List()
        {
            return _payloads.AsEnumerable();
        }
    }
}