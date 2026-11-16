using MediatR;
using Ardalis.Result;
using EduGame.Domain.Entities;

namespace EduGame.Application.Auth.Registration;

public record RegistrationCommand<T, D>(BaseRegistrationDto UserDto) :  IRequest<Result<string>>
    where T : BaseUser
    where D : BaseRegistrationDto;