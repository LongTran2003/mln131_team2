using LotoMln.DataAccess.IRepositories;
using Microsoft.AspNetCore.SignalR;

namespace LotoMln.API.Hubs;

public class GameHub(
    IUnitOfWork uow,                                    // ← inject
    ILogger<GameHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var http = Context.GetHttpContext();
        var roomCode = http?.Request.Query["roomCode"].ToString();
        var clientId = http?.Request.Query["clientId"].ToString();

        if (string.IsNullOrWhiteSpace(roomCode))
        {
            await Clients.Caller.SendAsync("ConnectionRejected",
                new { reason = "Missing roomCode in query string" });
            logger.LogWarning("Connection {ConnId} rejected: missing roomCode", Context.ConnectionId);
            Context.Abort();
            return;
        }

        var exists = await uow.Rooms.ExistsAsync(roomCode);
        if (!exists)
        {
            await Clients.Caller.SendAsync("ConnectionRejected",
                new { reason = $"Room '{roomCode}' không tồn tại" });
            logger.LogWarning("Connection {ConnId} rejected: room {Code} not found",
                Context.ConnectionId, roomCode);
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{roomCode}");
        logger.LogInformation("Connection {ConnId} (player {ClientId}) joined room:{Code}",
            Context.ConnectionId, clientId, roomCode);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Connection {ConnId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public Task JoinRoomGroup(string roomCode)
        => Groups.AddToGroupAsync(Context.ConnectionId, $"room:{roomCode}");

    public Task LeaveRoomGroup(string roomCode)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room:{roomCode}");

    public string Ping() => "pong";
}