using LotoMln.Models.Entities;
using LotoMln.Services.Helpers;
using LotoMln.Services.IServices;

namespace LotoMln.Services.Services;

public class CardGeneratorService : ICardGeneratorService
{
    public List<Card> GenerateCardsForRoom(string roomCode, int count)
    {
        var cards = new List<Card>(count);
        for (int i = 0; i < count; i++)
        {
            cards.Add(new Card
            {
                Id = Guid.NewGuid(),
                RoomCode = roomCode,
                Grid = CardGridGenerator.Generate(),
                OwnerId = null
            });
        }
        return cards;
    }
}