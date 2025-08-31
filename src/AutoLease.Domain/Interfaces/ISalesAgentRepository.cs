using AutoLease.Domain.Entities;

namespace AutoLease.Domain.Interfaces;

public interface ISalesAgentRepository
{
    Task<SalesAgent?> GetByIdAsync(int id);
    Task<SalesAgent?> GetByUserIdAsync(string userId);
    Task<SalesAgent?> GetByEmailAsync(string email);
    Task<IEnumerable<SalesAgent>> GetAllAsync();
    Task<IEnumerable<SalesAgent>> GetActiveSalesAgentsAsync();
    Task<IEnumerable<SalesAgent>> GetFilteredSalesAgentsAsync(string? searchTerm, string? department, decimal? minCommissionRate, int pageNumber, int pageSize, string sortBy, bool sortDescending);
    Task<int> GetTotalCountAsync(string? searchTerm, string? department, decimal? minCommissionRate);
    Task<SalesAgent> AddAsync(SalesAgent salesAgent);
    Task UpdateAsync(SalesAgent salesAgent);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UserIdExistsAsync(string userId);
}