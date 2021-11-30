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
        private static readonly Dictionary<string, List<string>> RoomToIds = new Dictionary<string, List<string>>();
        private static readonly Dictionary<string, string> IdToRoomName = new Dictionary<string, string>();
        private static readonly List<string> OpenRooms = new List<string>();

        public async Task SendMessage(string groupName, string name, string message)
        {
            var epochTicks = new TimeSpan(new DateTime(1970, 1, 1).Ticks);
            var unixTicks = new TimeSpan(DateTime.UtcNow.Ticks) - epochTicks;
            var unixTime = (int) MathF.Floor((float) unixTicks.TotalSeconds);

            await Clients.Group(groupName).SendAsync("OnMessageReceived", name, message, unixTime);
        }

        public async Task RequestNewRoom(bool isOpen)
        {
            var groupName = Guid.NewGuid().ToString();
            RoomToIds.Add(groupName, new List<string>());
            if (isOpen) OpenRooms.Add(groupName);
            await ConnectToRoom(Context.ConnectionId, groupName);
        }

        public async Task JoinRoom(string groupName)
        {
            if (RoomToIds.ContainsKey(groupName))
                await ConnectToRoom(Context.ConnectionId, groupName);
            else
                await Clients.Caller.SendAsync("OnRoomConnectionFail");
        }

        public async Task JoinRandomRoom()
        {
            if (OpenRooms.Count == 0)
            {
                await Clients.Caller.SendAsync("OnRoomConnectionFail");
                return;
            }

            var randomGroupNumber = new Random().Next() % OpenRooms.Count;
            var groupName = OpenRooms.ToArray()[randomGroupNumber];

            await ConnectToRoom(Context.ConnectionId, groupName);
        }

        private async Task ConnectToRoom(string contextId, string groupName)
        {
            await Groups.AddToGroupAsync(contextId, groupName);
            RoomToIds[groupName].Add(contextId);
            IdToRoomName.Add(contextId, groupName);
            var isGroupOpen = OpenRooms.Contains(groupName);
            
            await Clients.Caller.SendAsync("OnRoomConnect", groupName, isGroupOpen);
        }

        public async Task LeaveRoom(string groupName)
        {
            await Clients.Caller.SendAsync("OnLeaveRoom", groupName);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            OnLeftRoom(groupName);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (!IdToRoomName.ContainsKey(Context.ConnectionId))
                return base.OnDisconnectedAsync(exception);
            var groupName = IdToRoomName[Context.ConnectionId];
            OnLeftRoom(groupName);
            return base.OnDisconnectedAsync(exception);
        }

        private void OnLeftRoom(string groupName)
        {
            RoomToIds[groupName].Remove(Context.ConnectionId);
            IdToRoomName.Remove(Context.ConnectionId);
            
            if (RoomToIds[groupName].Count >= 1) return;
            RoomToIds.Remove(groupName);
            OpenRooms.Remove(groupName);
        }
    }
}