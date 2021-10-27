using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ChatServer
{
    public class ChatHub : Hub
    {
        public async Task Send(string name, string message)
        {
            var epochTicks = new TimeSpan(new DateTime(1970, 1, 1).Ticks);
            var unixTicks = new TimeSpan(DateTime.UtcNow.Ticks) - epochTicks;
            var unixTime = MathF.Floor((float) unixTicks.TotalSeconds);
            
            await Clients.All.SendAsync("Send", name, message, unixTime);
        }
    }
}