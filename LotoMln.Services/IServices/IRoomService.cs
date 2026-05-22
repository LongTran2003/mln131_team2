using LotoMln.Models.DTOs;

namespace LotoMln.Services.IServices;

public interface IRoomService
{
    Task<CreateRoomResponse> CreateRoomAsync(CreateRoomRequest req, CancellationToken ct = default);
    Task<JoinRoomResponse> JoinRoomAsync(string roomCode, JoinRoomRequest req, CancellationToken ct = default);
    Task<PickCardResponse> PickCardAsync(PickCardRequest req, CancellationToken ct = default);
    Task<RoomDto?> GetRoomAsync(string code, CancellationToken ct = default);
    Task<List<CardDto>> GetAvailableCardsAsync(string roomCode, CancellationToken ct = default);
    Task<List<PlayerDto>> GetPlayersAsync(string roomCode, CancellationToken ct = default);
    Task<bool> UnpickCardAsync(string roomCode, Guid playerId, CancellationToken ct = default);
    Task<List<CardDto>> GetAllCardsAsync(string roomCode, CancellationToken ct = default);
}