using EduGame.Domain.Entities;
using EduGame.Domain.Enums;

namespace EduGame.Application.Chat.SaveMessage;

public record SaveMessageResponseDto(Message Message, UserRole? UserRole);