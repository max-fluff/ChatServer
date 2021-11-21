#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ChatServer
{
    public class ChatHub : Hub
    {
        private readonly Dictionary<string, List<string>> _roomToIds = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, string> _idToRoomName = new Dictionary<string, string>();

        public async Task SendMessage(string groupName, string name, string message)
        {
            var epochTicks = new TimeSpan(new DateTime(1970, 1, 1).Ticks);
            var unixTicks = new TimeSpan(DateTime.UtcNow.Ticks) - epochTicks;
            var unixTime = (int) MathF.Floor((float) unixTicks.TotalSeconds);

            await Clients.Group(groupName).SendAsync("OnMessageReceived", name, message, unixTime);
        }

        public async Task RequestNewRoom()
        {
            var groupName = Guid.NewGuid().ToString();
            _roomToIds.Add(groupName, new List<string>());
            await ConnectToRoom(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("OnRoomConnect", groupName);
        }

        public async Task JoinRoom(string groupName)
        {
            if (_roomToIds.ContainsKey(groupName))
            {
                await ConnectToRoom(Context.ConnectionId, groupName);
                await Clients.Caller.SendAsync("OnRoomConnect", groupName);
            }
            else
                await Clients.Caller.SendAsync("OnRoomConnectionFail");
        }

        public async Task JoinRandomRoom()
        {
            if (_roomToIds.Count == 0)
            {
                await Clients.Caller.SendAsync("OnRoomConnectionFail");
                return;
            }
            
            var randomGroupNumber = new Random().Next() % _roomToIds.Count;
            var groupName = _roomToIds.Keys.ToArray()[randomGroupNumber];

            await ConnectToRoom(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("OnRoomConnect", groupName);
        }

        private async Task ConnectToRoom(string contextId, string groupName)
        {
            await Groups.AddToGroupAsync(contextId, groupName);
            _roomToIds[groupName].Add(contextId);
            _idToRoomName.Add(contextId, groupName);
        }

        public async Task LeaveRoom(string groupName)
        {
            await Clients.Caller.SendAsync("OnLeaveRoom", groupName);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            OnLeftRoom(groupName);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var groupName = _idToRoomName[Context.ConnectionId];
            OnLeftRoom(groupName);
            return base.OnDisconnectedAsync(exception);
        }

        private void OnLeftRoom(string groupName)
        {
            _roomToIds[groupName].Remove(Context.ConnectionId);
            _idToRoomName.Remove(Context.ConnectionId);
            if (_roomToIds[groupName].Count < 1)
                _roomToIds.Remove(groupName);
        }
    }
}