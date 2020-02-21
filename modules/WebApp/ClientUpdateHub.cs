/*

The ClientUpdateHub is the SignalR implementation which allows HttpModuleClient to send a message
to the website (the client) that new information is available and the cloud to be able to send
messages back if needed (not implemented in this demo).
*/

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using System.Text;
using System;

namespace WebApp
{
    public class ClientUpdateHub : Hub
    {
        private IBackgroundPayloadQueue PayloadQueue { get; set; }
        public ClientUpdateHub(IBackgroundPayloadQueue payloadQueue)
        {
            this.PayloadQueue = payloadQueue;
        }
    }
}