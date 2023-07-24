using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;

namespace eShop
{
    [Authorize]
    public class PostingHub : Hub
    {
        public static void SendProgress(string progressMessage)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<PostingHub>();
            string dateTime = DateTime.Now.ToString("T");

            hubContext.Clients.All.AddProgress(dateTime, progressMessage);
        }

        public void BroadcastProgress(string progressMessage)
        {
            string dateTime = DateTime.Now.ToString("T");

            Clients.All.AddProgress(dateTime, progressMessage);
        }

        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }
    }
}