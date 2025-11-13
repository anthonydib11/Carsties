using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.RequestHelper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Auction,AuctionDto>().IncludeMembers(s => s.Item);
            CreateMap<Item, AuctionDto>();
            CreateMap<CreateAuctionDto, Auction>()
                .ForMember(d=> d.Item,o => o.MapFrom(s => s));
            CreateMap<CreateAuctionDto, Item>();
              
        }
    }
}
