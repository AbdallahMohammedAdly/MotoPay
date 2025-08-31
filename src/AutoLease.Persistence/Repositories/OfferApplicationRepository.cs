using AutoLease.Domain.Entities;
using AutoLease.Domain.Interfaces;
using AutoLease.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoLease.Persistence.Repositories;

public class OfferApplicationRepository : IOfferApplicationRepository
{
    private readonly AutoLeaseDbContext _context;

    public OfferApplicationRepository(AutoLeaseDbContext context)
    {
        _context = context;
    }

    public async Task<OfferApplication?> GetByIdAsync(int id)
    {
        return await _context.OfferApplications
            .Include(a => a.Offer)
                .ThenInclude(o => o.Car)
            .Include(a => a.User)
            .Include(a => a.ReviewedByUser)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<OfferApplication>> GetByOfferIdAsync(int offerId)
    {
        return await _context.OfferApplications
            .Include(a => a.User)
            .Include(a => a.ReviewedByUser)
            .Where(a => a.OfferId == offerId)
            .OrderByDescending(a => a.ApplicationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<OfferApplication>> GetByUserIdAsync(string userId)
    {
        return await _context.OfferApplications
            .Include(a => a.Offer)
                .ThenInclude(o => o.Car)
            .Include(a => a.ReviewedByUser)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.ApplicationDate)
            .ToListAsync();
    }

    public async Task<OfferApplication?> GetByOfferAndUserAsync(int offerId, string userId)
    {
        return await _context.OfferApplications
            .Include(a => a.Offer)
                .ThenInclude(o => o.Car)
            .Include(a => a.User)
            .Include(a => a.ReviewedByUser)
            .FirstOrDefaultAsync(a => a.OfferId == offerId && a.UserId == userId);
    }

    public async Task<IEnumerable<OfferApplication>> GetPendingApplicationsAsync()
    {
        return await _context.OfferApplications
            .Include(a => a.Offer)
                .ThenInclude(o => o.Car)
            .Include(a => a.User)
            .Where(a => a.Status == OfferApplicationStatus.Pending)
            .OrderBy(a => a.ApplicationDate)
            .ToListAsync();
    }

    public async Task<OfferApplication> AddAsync(OfferApplication application)
    {
        _context.OfferApplications.Add(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task UpdateAsync(OfferApplication application)
    {
        _context.OfferApplications.Update(application);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int offerId, string userId)
    {
        return await _context.OfferApplications
            .AnyAsync(a => a.OfferId == offerId && a.UserId == userId);
    }
}