using AutoMapper;
using EduGame.Domain.Entities;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Chat.SaveMessage;

public class MessageMappingProfile : Profile
{
    public MessageMappingProfile()
    {
        CreateMap<MessageDto, Message>()
            .ForMember(dest => dest.DeliveredCount, opt => opt.Ignore())
            .ForMember(dest => dest.ReadCount, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore());  
    }
}