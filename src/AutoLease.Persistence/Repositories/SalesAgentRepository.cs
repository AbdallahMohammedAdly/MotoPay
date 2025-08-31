using AutoLease.Domain.Entities;
using AutoLease.Domain.Interfaces;
using AutoLease.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoLease.Persistence.Repositories;

public class SalesAgentRepository : ISalesAgentRepository
{
    private readonly AutoLeaseDbContext _context;

    public SalesAgentRepository(AutoLeaseDbContext context)
    {
        _context = context;
    }

    public async Task<SalesAgent?> GetByIdAsync(int id)
    {
        return await _context.SalesAgents
            .Include(s => s.User)
            .Include(s => s.AssignedCars)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<SalesAgent?> GetByUserIdAsync(string userId)
    {
        return await _context.SalesAgents
            .Include(s => s.User)
            .Include(s => s.AssignedCars)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<SalesAgent?> GetByEmailAsync(string email)
    {
        return await _context.SalesAgents
            .Include(s => s.User)
            .Include(s => s.AssignedCars)
            .FirstOrDefaultAsync(s => s.Email == email);
    }

    public async Task<IEnumerable<SalesAgent>> GetAllAsync()
    {
        return await _context.SalesAgents
            .Include(s => s.User)
            .Include(s => s.AssignedCars)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<SalesAgent>> GetActiveSalesAgentsAsync()
    {
        return await _context.SalesAgents
            .Include(s => s.User)
            .Include(s => s.AssignedCars)
            .Where(s => s.IsActive)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
    }

    public async Task<SalesAgent> AddAsync(SalesAgent salesAgent)
    {
        _context.SalesAgents.Add(salesAgent);
        await _context.SaveChangesAsync();
        return salesAgent;
    }

    public async Task UpdateAsync(SalesAgent salesAgent)
    {
        _context.SalesAgents.Update(salesAgent);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var salesAgent = await _context.SalesAgents.FindAsync(id);
        if (salesAgent != null)
        {
            _context.SalesAgents.Remove(salesAgent);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.SalesAgents.AnyAsync(s => s.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.SalesAgents.AnyAsync(s => s.Email == email);
    }

    public async Task<IEnumerable<SalesAgent>> GetFilteredSalesAgentsAsync(string? searchTerm, string? department, 
        decimal? minCommissionRate, int pageNumber, int pageSize, string sortBy, bool sortDescending)
    {
        var query = _context.SalesAgents
            .Include(s => s.User)
            .Include(s => s.AssignedCars)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(s => s.FirstName.Contains(searchTerm) || 
                                   s.LastName.Contains(searchTerm) ||
                                   s.Email.Contains(searchTerm) ||
                                   s.Department.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(department))
        {
            query = query.Where(s => s.Department == department);
        }

        if (minCommissionRate.HasValue)
        {
            query = query.Where(s => s.CommissionRate >= minCommissionRate);
        }

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "firstname" => sortDescending ? query.OrderByDescending(s => s.FirstName) : query.OrderBy(s => s.FirstName),
            "lastname" => sortDescending ? query.OrderByDescending(s => s.LastName) : query.OrderBy(s => s.LastName),
            "department" => sortDescending ? query.OrderByDescending(s => s.Department) : query.OrderBy(s => s.Department),
            "commissionrate" => sortDescending ? query.OrderByDescending(s => s.CommissionRate) : query.OrderBy(s => s.CommissionRate),
            "assignedcars" => sortDescending ? query.OrderByDescending(s => s.AssignedCars.Count) : query.OrderBy(s => s.AssignedCars.Count),
            _ => sortDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt)
        };

        // Apply pagination
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? searchTerm, string? department, decimal? minCommissionRate)
    {
        var query = _context.SalesAgents.AsQueryable();

        // Apply same filters as GetFilteredSalesAgentsAsync
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(s => s.FirstName.Contains(searchTerm) || 
                                   s.LastName.Contains(searchTerm) ||
                                   s.Email.Contains(searchTerm) ||
                                   s.Department.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(department))
        {
            query = query.Where(s => s.Department == department);
        }

        if (minCommissionRate.HasValue)
        {
            query = query.Where(s => s.CommissionRate >= minCommissionRate);
        }

        return await query.CountAsync();
    }

    public async Task<bool> UserIdExistsAsync(string userId)
    {
        return await _context.SalesAgents.AnyAsync(s => s.UserId == userId);
    }
}