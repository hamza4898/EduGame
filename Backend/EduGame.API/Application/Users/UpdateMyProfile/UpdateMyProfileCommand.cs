using MediatR;
using Ardalis.Result;
using EduGame.Application.Users.GetMyBusinessPassport;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Users.UpdateMyProfile;

public sealed record UpdateMyProfileCommand(string UserId, UpdateMyProfileRequestDto Request) : IRequest<Result<MyBusinessPassportResponseDto>>;