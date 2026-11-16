using EduGame.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Infrastructure.Repositories;

public class Repository<T>(EduGameDbContext context) : IRepository<T> where T : class
{
    protected readonly EduGameDbContext Context = context; 

    public async Task<T?> GetById(int id) =>
        await Context.Set<T>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => EF.Property<int>(x, "Id") == id);

    public async Task<T?> GetById(string externalId) =>
        await Context.Set<T>()
            .FirstOrDefaultAsync(user => EF.Property<string>(user, "Id") == externalId);

    public async Task Add(T entity) =>
        await Context.Set<T>().AddAsync(entity);

    public async Task Save() => 
        await Context.SaveChangesAsync();
}