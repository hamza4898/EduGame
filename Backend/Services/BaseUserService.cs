using EduGame.Entities;
using AutoMapper;
using BCrypt.Net;
using EduGame.DTOs;

namespace EduGame.Services
{
    public class BaseUserService<T, D> : IBaseUserService<T, D>
        where T: class
        where D: BaseRegistrationDTO
    {
        private readonly EFCoreDbContext _eduGameDbContext;
        private readonly IMapper _mapper;

        public BaseUserService(EFCoreDbContext eduGameDbContext, IMapper mapper)
        {
            _eduGameDbContext = eduGameDbContext;
            _mapper = mapper;
        }

        public async Task CreateUser(D userDTO)
        {
            var user = _mapper.Map<T>(userDTO);

            ((dynamic)user).PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);

            _eduGameDbContext.Add(user);
            await _eduGameDbContext.SaveChangesAsync();
        }
    }
}