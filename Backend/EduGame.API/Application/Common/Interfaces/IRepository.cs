using EduGame.Domain.Entities;

namespace EduGame.Application.Common.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetById(int Id);

    Task<T?> GetById(string externalId);

    Task Add(T user);

    Task Save();
}