using LotoMln.Models.DTOs;
using LotoMln.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace LotoMln.API.Controllers;

[ApiController]
[Route("api/Rooms/{code}/game")]
public class GameController(IGameEngineService engine) : ControllerBase
{
    [HttpGet("state")]
    public async Task<IActionResult> GetState(string code, CancellationToken ct)
    {
        var state = await engine.GetGameStateAsync(code, ct);
        return state == null ? NotFound() : Ok(state);
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start(
        string code, [FromQuery] Guid initiatorId, CancellationToken ct)
    {
        try { return Ok(await engine.StartGameAsync(code, initiatorId, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("next-drawer")]
    public async Task<IActionResult> NextDrawer(string code, CancellationToken ct)
    {
        try { return Ok(await engine.SelectNextDrawerAsync(code, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("pick-slot")]
    public async Task<IActionResult> PickSlot(
        string code, [FromBody] DrawerPickRequest req, CancellationToken ct)
    {
        try { return Ok(await engine.OnDrawerPicksSlotAsync(code, req, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("submit-answer")]
    public async Task<IActionResult> SubmitAnswer(
        string code, [FromBody] SubmitAnswerRequest req, CancellationToken ct)
    {
        try { return Ok(await engine.OnDrawerAnswersAsync(code, req, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("steal")]
    public async Task<IActionResult> Steal(
        string code, [FromBody] StealAttemptRequest req, CancellationToken ct)
    {
        try { return Ok(await engine.RecordStealAttemptAsync(code, req, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("resolve-steal")]
    public async Task<IActionResult> ResolveSteal(string code, CancellationToken ct)
    {
        try { return Ok(await engine.ResolveStealAsync(code, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("mark")]
    public async Task<IActionResult> Mark(
        string code, [FromBody] MarkNumberRequest req, CancellationToken ct)
    {
        try { return Ok(new { marked = await engine.MarkNumberAsync(code, req, ct) }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("claim-kinh")]
    public async Task<IActionResult> ClaimKinh(
        string code, [FromBody] ClaimKinhRequest req, CancellationToken ct)
    {
        try { return Ok(await engine.ClaimKinhAsync(code, req, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}