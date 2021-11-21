using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ChatServer
{
    public class ChatHub : Hub
    {
        public async Task Send(string groupName, string name, string message)
        {
            var epochTicks = new TimeSpan(new DateTime(1970, 1, 1).Ticks);
            var unixTicks = new TimeSpan(DateTime.UtcNow.Ticks) - epochTicks;
            var unixTime = (int) MathF.Floor((float) unixTicks.TotalSeconds);

            await Clients.Group(groupName).SendAsync("Send", name, message, unixTime);
        }

        public async Task RequestNewRoom()
        {
            var groupName = Guid.NewGuid().ToString();
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("OnRoomConnect", groupName);
        }

        public async Task JoinRoom(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("OnRoomConnect", groupName);
        }

        public async Task LeaveRoom(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("OnLeaveRoom", groupName);
        }
    }
}