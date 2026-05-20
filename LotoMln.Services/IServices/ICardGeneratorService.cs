using LotoMln.Models.Entities;

namespace LotoMln.Services.IServices;

public interface ICardGeneratorService
{
    List<Card> GenerateCardsForRoom(string roomCode, int count);
}