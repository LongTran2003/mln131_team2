using AutoMapper;
using LotoMln.Models.DTOs;
using LotoMln.Models.Entities;

namespace LotoMln.Services.Mapping;

public class GameMappingProfile : Profile
{
    public GameMappingProfile()
    {
        // Player → PlayerDto (tự động match field cùng tên)
        CreateMap<Player, PlayerDto>()
            .ForCtorParam(nameof(PlayerDto.IsHost), opt => opt.MapFrom(_ => false));

        // Room → RoomDto (PlayerCount tính từ navigation Players)
        CreateMap<Room, RoomDto>()
            .ForCtorParam(nameof(RoomDto.PlayerCount),
                opt => opt.MapFrom(src => src.Players.Count));

        // Card → CardDto (IsAvailable = OwnerId == null)
        CreateMap<Card, CardDto>()
            .ForCtorParam(nameof(CardDto.IsAvailable),
                opt => opt.MapFrom(src => src.OwnerId == null));
    }
}