using LotoMln.Models.DTOs;
using LotoMln.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace LotoMln.API.Controllers;

[ApiController]
[Route("api/room")]
public class RoomsController(IRoomService roomService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateRoom(
        [FromBody] CreateRoomRequest req, CancellationToken ct)
    {
        try
        {
            var result = await roomService.CreateRoomAsync(req, ct);
            return CreatedAtAction(nameof(GetRoom), new { code = result.RoomCode }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetRoom(string code, CancellationToken ct)
    {
        var room = await roomService.GetRoomAsync(code, ct);
        return room == null ? NotFound() : Ok(room);
    }

    [HttpPost("{code}/join")]
    public async Task<IActionResult> JoinRoom(
    string code, [FromBody] JoinRoomRequest req, CancellationToken ct)
    {
        try
        {
            var result = await roomService.JoinRoomAsync(code, req, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{code}/cards/available")]
    public async Task<IActionResult> GetAvailableCards(string code, CancellationToken ct)
    {
        var cards = await roomService.GetAvailableCardsAsync(code, ct);
        return Ok(cards);
    }

    [HttpPost("{code}/cards/{cardId:guid}/pick")]
    public async Task<IActionResult> PickCard(
        string code, Guid cardId, [FromQuery] Guid playerId, CancellationToken ct)
    {
        try
        {
            var result = await roomService.PickCardAsync(
                new PickCardRequest(code, playerId, cardId), ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{code}/players")]
    public async Task<IActionResult> GetPlayers(string code, CancellationToken ct)
    {
        var players = await roomService.GetPlayersAsync(code, ct);
        return Ok(players);
    }
}