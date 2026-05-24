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

    [HttpPost("spin-wheel")]
    public async Task<IActionResult> SpinWheel(
        string code, [FromBody] SpinWheelRequest req, CancellationToken ct)
    {
        try { return Ok(await engine.SpinWheelAsync(code, req.HostId, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("select-answerer")]
    public async Task<IActionResult> SelectAnswerer(
        string code, [FromBody] SelectAnswererRequest req, CancellationToken ct)
    {
        try
        {
            await engine.SelectAnswererAsync(code, req.HostId, req.PlayerId, ct);
            return Ok();
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("submit-answer")]
    public async Task<IActionResult> SubmitAnswer(
        string code, [FromBody] SubmitAnswerRequest req, CancellationToken ct)
    {
        try { return Ok(await engine.OnDrawerAnswersAsync(code, req, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("skip-slot")]
    public async Task<IActionResult> SkipSlot(
        string code, [FromBody] SkipSlotRequest req, CancellationToken ct)
    {
        try
        {
            await engine.SkipSlotAsync(code, req.HostId, ct);
            return Ok();
        }
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