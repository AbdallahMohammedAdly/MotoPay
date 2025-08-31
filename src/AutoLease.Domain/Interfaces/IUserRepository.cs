using AutoLease.Domain.Entities;

namespace AutoLease.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<bool> EmailExistsAsync(string email);
}