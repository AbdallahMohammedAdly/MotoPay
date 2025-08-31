using AutoLease.Domain.Entities;
using AutoLease.Domain.Interfaces;
using AutoLease.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoLease.Persistence.Repositories;

public class CarRepository : ICarRepository
{
    private readonly AutoLeaseDbContext _context;

    public CarRepository(AutoLeaseDbContext context)
    {
        _context = context;
    }

    public async Task<Car?> GetByIdAsync(int id)
    {
        return await _context.Cars
            .Include(c => c.Owner)
            .Include(c => c.SalesAgent)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Car?> GetByVinNumberAsync(string vinNumber)
    {
        return await _context.Cars
            .Include(c => c.Owner)
            .Include(c => c.SalesAgent)
            .FirstOrDefaultAsync(c => c.VinNumber == vinNumber);
    }

    public async Task<IEnumerable<Car>> GetAllAsync()
    {
        return await _context.Cars
            .Include(c => c.Owner)
            .Include(c => c.SalesAgent)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Car>> GetAvailableCarsAsync()
    {
        return await _context.Cars
            .Include(c => c.SalesAgent)
            .Where(c => c.IsAvailable)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Car>> GetCarsByOwnerIdAsync(string ownerId)
    {
        return await _context.Cars
            .Include(c => c.SalesAgent)
            .Where(c => c.OwnerId == ownerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Car>> GetCarsBySalesAgentIdAsync(int salesAgentId)
    {
        return await _context.Cars
            .Include(c => c.Owner)
            .Where(c => c.SalesAgentId == salesAgentId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Car> AddAsync(Car car)
    {
        _context.Cars.Add(car);
        await _context.SaveChangesAsync();
        return car;
    }

    public async Task UpdateAsync(Car car)
    {
        _context.Cars.Update(car);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car != null)
        {
            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Cars.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> VinNumberExistsAsync(string vinNumber)
    {
        return await _context.Cars.AnyAsync(c => c.VinNumber == vinNumber);
    }

    public async Task<IEnumerable<Car>> GetFilteredCarsAsync(string? searchTerm, string? make, int? year, 
        decimal? minPrice, decimal? maxPrice, bool? isAvailable, int? salesAgentId, 
        int pageNumber, int pageSize, string sortBy, bool sortDescending)
    {
        var query = _context.Cars
            .Include(c => c.Owner)
            .Include(c => c.SalesAgent)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(c => c.Make.Contains(searchTerm) || 
                                   c.Model.Contains(searchTerm) || 
                                   c.VinNumber.Contains(searchTerm) ||
                                   c.Description.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(make))
        {
            query = query.Where(c => c.Make == make);
        }

        if (year.HasValue)
        {
            query = query.Where(c => c.Year == year);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(c => c.Price >= minPrice);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(c => c.Price <= maxPrice);
        }

        if (isAvailable.HasValue)
        {
            query = query.Where(c => c.IsAvailable == isAvailable);
        }

        if (salesAgentId.HasValue)
        {
            query = query.Where(c => c.SalesAgentId == salesAgentId);
        }

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "make" => sortDescending ? query.OrderByDescending(c => c.Make) : query.OrderBy(c => c.Make),
            "model" => sortDescending ? query.OrderByDescending(c => c.Model) : query.OrderBy(c => c.Model),
            "year" => sortDescending ? query.OrderByDescending(c => c.Year) : query.OrderBy(c => c.Year),
            "price" => sortDescending ? query.OrderByDescending(c => c.Price) : query.OrderBy(c => c.Price),
            _ => sortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
        };

        // Apply pagination
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? searchTerm, string? make, int? year, 
        decimal? minPrice, decimal? maxPrice, bool? isAvailable, int? salesAgentId)
    {
        var query = _context.Cars.AsQueryable();

        // Apply same filters as GetFilteredCarsAsync
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(c => c.Make.Contains(searchTerm) || 
                                   c.Model.Contains(searchTerm) || 
                                   c.VinNumber.Contains(searchTerm) ||
                                   c.Description.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(make))
        {
            query = query.Where(c => c.Make == make);
        }

        if (year.HasValue)
        {
            query = query.Where(c => c.Year == year);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(c => c.Price >= minPrice);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(c => c.Price <= maxPrice);
        }

        if (isAvailable.HasValue)
        {
            query = query.Where(c => c.IsAvailable == isAvailable);
        }

        if (salesAgentId.HasValue)
        {
            query = query.Where(c => c.SalesAgentId == salesAgentId);
        }

        return await query.CountAsync();
    }
}