using EduGame.Data;
using AutoMapper;
using EduGame.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EduGame.Entities;

namespace EduGame.Services
{
    public class RegistrationService<T, D> : IRegistrationService<T, D>
        where T: class
        where D: BaseRegistrationDto
    {
        private readonly EduGameDbContext _eduGameDbContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistrationService(EduGameDbContext eduGameDbContext, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _eduGameDbContext = eduGameDbContext;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<T> CreateUser(D userDto)
        {
            var identityUser = _mapper.Map<ApplicationUser>(userDto);
            var result = await _userManager.CreateAsync(identityUser, userDto.Password);       

            var userProfile = _mapper.Map<T>(userDto);

            ((dynamic)userProfile).ExternalId = identityUser.Id;

            await _eduGameDbContext.Set<T>().AddAsync(userProfile);
            await _eduGameDbContext.SaveChangesAsync();

            return userProfile;
        }

        public async Task<T> GetUserByExternalId(Guid externalId)
        {
            return await _eduGameDbContext.Set<T>()
                .FirstOrDefaultAsync(user => EF.Property<Guid>(user, "ExternalId") == externalId);
        }
    }
}