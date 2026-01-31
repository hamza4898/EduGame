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

        public async Task<T> CreateUser(D userDTO)
        {
            var userEntity = _mapper.Map<T>(userDTO);

            ((dynamic)userEntity).PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);

            _eduGameDbContext.Add(userEntity);
            await _eduGameDbContext.SaveChangesAsync();

            return userEntity;
        }

        public async Task<T> GetUserById(int id)
        {
            return (await _eduGameDbContext.Set<T>().FindAsync(id))!;
        }
    }
}