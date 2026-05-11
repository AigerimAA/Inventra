using AutoMapper;
using Inventra.Application.DTOs;
using Inventra.Domain.Entities;

namespace Inventra.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Inventory, InventoryDto>()
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner != null ? src.Owner.UserName : null))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.InventoryTags.Select(it => it.Tag.Name).ToList()))
                .ForMember(dest => dest.ItemsCount, opt => opt.MapFrom(src => src.Items.Count));

            CreateMap<Item, ItemDto>()
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.UserName : null))
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.Likes.Count))
                .ForMember(dest => dest.IsLikedByCurrentUser, opt => opt.Ignore());

            CreateMap<Category, CategoryDto>();
        }
    }
}
