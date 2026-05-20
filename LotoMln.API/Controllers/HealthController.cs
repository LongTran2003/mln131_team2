using LotoMln.DataAccess.DBContext;
using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.API.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController(IUnitOfWork uow, AppDbContext db) : ControllerBase
{
    [HttpGet]
    public IActionResult Ping() => Ok(new { status = "healthy" });

    [HttpGet("db-ping")]
    public async Task<IActionResult> DbPing(CancellationToken ct)
    {
        var canConnect = await db.Database.CanConnectAsync(ct);
        return Ok(new { connected = canConnect });
    }

    [HttpPost("uow-test")]
    public async Task<IActionResult> UowTest(CancellationToken ct)
    {
        // Tạo room + player + card → save → đọc lại để verify UoW pattern
        var code = Random.Shared.Next(100000, 999999).ToString();
        var hostId = Guid.NewGuid();

        await uow.Rooms.AddAsync(new Room { Code = code, HostId = hostId }, ct);
        await uow.Players.AddAsync(new Player
        {
            Id = hostId,
            RoomCode = code,
            Name = "TestHost"
        }, ct);
        await uow.Cards.AddAsync(new Card
        {
            Id = Guid.NewGuid(),
            RoomCode = code,
            Grid = [
                [1, 9, 17, 25, 33],
                [2, 10, 18, 26, 34],
                [3, 11, 19, 27, 35],
                [4, 12, 20, 28, 36],
                [5, 13, 21, 29, 37]
            ]
        }, ct);

        await uow.SaveChangesAsync(ct);

        // Verify
        var room = await uow.Rooms.GetWithDetailsAsync(code, ct);
        var playerCount = await uow.Players.CountByRoomCodeAsync(code, ct);
        var cards = await uow.Cards.GetByRoomCodeAsync(code, ct);

        return Ok(new
        {
            roomCode = code,
            roomExists = room != null,
            roomState = room?.State.ToString(),
            playerCount,
            cardCount = cards.Count,
            firstCardGrid = cards.FirstOrDefault()?.Grid
        });
    }

    [HttpDelete("uow-test/{code}")]
    public async Task<IActionResult> CleanupTest(string code, CancellationToken ct)
    {
        var room = await uow.Rooms.GetByCodeAsync(code, ct);
        if (room == null) return NotFound();
        uow.Rooms.Remove(room);
        await uow.SaveChangesAsync(ct);
        return NoContent();    // cascade delete sẽ tự xóa player + card
    }
}