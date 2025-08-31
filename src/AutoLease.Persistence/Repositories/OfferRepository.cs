using AutoLease.Domain.Entities;
using AutoLease.Domain.Interfaces;
using AutoLease.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoLease.Persistence.Repositories;

public class OfferRepository : IOfferRepository
{
    private readonly AutoLeaseDbContext _context;

    public OfferRepository(AutoLeaseDbContext context)
    {
        _context = context;
    }

    public async Task<Offer?> GetByIdAsync(int id)
    {
        return await _context.Offers
            .Include(o => o.Car)
            .Include(o => o.SalesAgent)
            .Include(o => o.Applications)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Offer>> GetAllAsync()
    {
        return await _context.Offers
            .Include(o => o.Car)
            .Include(o => o.SalesAgent)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Offer>> GetActiveOffersAsync()
    {
        return await _context.Offers
            .Include(o => o.Car)
            .Include(o => o.SalesAgent)
            .Where(o => o.IsActive && o.StartDate <= DateTime.UtcNow && o.EndDate >= DateTime.UtcNow)
            .OrderByDescending(o => o.DiscountPercentage)
            .ToListAsync();
    }

    public async Task<IEnumerable<Offer>> GetOffersByCarIdAsync(int carId)
    {
        return await _context.Offers
            .Include(o => o.Car)
            .Include(o => o.SalesAgent)
            .Where(o => o.CarId == carId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Offer>> GetOffersBySalesAgentIdAsync(int salesAgentId)
    {
        return await _context.Offers
            .Include(o => o.Car)
            .Include(o => o.SalesAgent)
            .Where(o => o.SalesAgentId == salesAgentId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Offer>> GetFilteredOffersAsync(string? searchTerm, decimal? minDiscount, 
        decimal? maxPrice, bool? isActive, int pageNumber, int pageSize)
    {
        var query = _context.Offers
            .Include(o => o.Car)
            .Include(o => o.SalesAgent)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(o => o.Title.Contains(searchTerm) || 
                                   o.Description.Contains(searchTerm) ||
                                   o.Car.Make.Contains(searchTerm) ||
                                   o.Car.Model.Contains(searchTerm));
        }

        if (minDiscount.HasValue)
        {
            query = query.Where(o => o.DiscountPercentage >= minDiscount);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(o => o.DiscountedPrice <= maxPrice);
        }

        if (isActive.HasValue)
        {
            if (isActive.Value)
            {
                query = query.Where(o => o.IsActive && 
                                       o.StartDate <= DateTime.UtcNow && 
                                       o.EndDate >= DateTime.UtcNow);
            }
            else
            {
                query = query.Where(o => !o.IsActive || o.EndDate < DateTime.UtcNow);
            }
        }

        // Apply pagination
        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? searchTerm, decimal? minDiscount, 
        decimal? maxPrice, bool? isActive)
    {
        var query = _context.Offers.AsQueryable();

        // Apply same filters as GetFilteredOffersAsync
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(o => o.Title.Contains(searchTerm) || 
                                   o.Description.Contains(searchTerm) ||
                                   o.Car.Make.Contains(searchTerm) ||
                                   o.Car.Model.Contains(searchTerm));
        }

        if (minDiscount.HasValue)
        {
            query = query.Where(o => o.DiscountPercentage >= minDiscount);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(o => o.DiscountedPrice <= maxPrice);
        }

        if (isActive.HasValue)
        {
            if (isActive.Value)
            {
                query = query.Where(o => o.IsActive && 
                                       o.StartDate <= DateTime.UtcNow && 
                                       o.EndDate >= DateTime.UtcNow);
            }
            else
            {
                query = query.Where(o => !o.IsActive || o.EndDate < DateTime.UtcNow);
            }
        }

        return await query.CountAsync();
    }

    public async Task<Offer> AddAsync(Offer offer)
    {
        _context.Offers.Add(offer);
        await _context.SaveChangesAsync();
        return offer;
    }

    public async Task UpdateAsync(Offer offer)
    {
        _context.Offers.Update(offer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var offer = await _context.Offers.FindAsync(id);
        if (offer != null)
        {
            _context.Offers.Remove(offer);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Offers.AnyAsync(o => o.Id == id);
    }
}