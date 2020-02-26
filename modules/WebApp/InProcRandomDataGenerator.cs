/*
This creates dummy image data for isolated HMI development.

See README.md for instructions on how to set the environment variables to use HttpModuleClient instead.
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WebApp
{
    public class InProcDotNetDataGenerator : BackgroundService
    {
        private readonly ILogger<InProcDotNetDataGenerator> _logger;

        private IBackgroundPayloadQueue PayloadQueue { get; }

        public InProcDotNetDataGenerator(IBackgroundPayloadQueue payloadQueue,
            ILogger<InProcDotNetDataGenerator> logger)
        {
            PayloadQueue = payloadQueue;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task QueuePayload(Payload data)
        {
            try
            {
                _logger.LogInformation($"{JsonConvert.SerializeObject(data)}");

                if (PayloadQueue.Count() > 10)
                    while (PayloadQueue.Count() > 10)
                        await PayloadQueue.DequeueAsync(new CancellationToken()); //throw away result

                // exit early if we dont have a deserialized element and a serial number
                if (data != null)
                {
                    _logger.LogInformation("Data Generator queueing in progress...");
                    PayloadQueue.QueuePayload(data);
                }
            }
            catch (AggregateException ex)
            {
                _logger.LogError($"Error processing message: {ex.Flatten()}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex}");
            }
        }

        private static readonly Random Rd = new Random();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("initializing random data generation.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var payload = new Payload{};
                payload.TimeStamp = DateTime.UtcNow;
                payload.Confidence = Rd.NextDouble();
                payload.IsAlive = Rd.Next() % 2 == 0;
                payload.TagKey = "dotnet";
                payload.TimeStamp = DateTime.UtcNow;

                await QueuePayload(payload);

                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            _logger.LogInformation("Cancellation requested.");
        }
    }
}