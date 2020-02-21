using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;

namespace WebApp
{
public class ClientNotifier : BackgroundService
    {
        private IBackgroundPayloadQueue PayloadQueue { get; set; }
        
        private IHubContext<ClientUpdateHub> HubContext { get; set; }
        public ClientNotifier(IBackgroundPayloadQueue payloadQueue,
            IHubContext<ClientUpdateHub> hubContext)
        {
            this.PayloadQueue = payloadQueue;
            this.HubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            /*hubContext allows ClientNotifier to act as a part of the signalR hub and send messages.  See this
            for more information: https://docs.microsoft.com/en-us/aspnet/core/signalr/hubcontext?view=aspnetcore-2.1 */

            var notifytask = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (PayloadQueue.Count() > 0)
                    {
                        await this.HubContext.Clients.All.SendAsync("NewData");
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }, stoppingToken);

            await Task.WhenAll(notifytask);

        }
    }
}