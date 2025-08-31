using AutoLease.Domain.Entities;

namespace AutoLease.Domain.Interfaces;

public interface IOfferApplicationRepository
{
    Task<OfferApplication?> GetByIdAsync(int id);
    Task<IEnumerable<OfferApplication>> GetByOfferIdAsync(int offerId);
    Task<IEnumerable<OfferApplication>> GetByUserIdAsync(string userId);
    Task<OfferApplication?> GetByOfferAndUserAsync(int offerId, string userId);
    Task<IEnumerable<OfferApplication>> GetPendingApplicationsAsync();
    Task<OfferApplication> AddAsync(OfferApplication application);
    Task UpdateAsync(OfferApplication application);
    Task<bool> ExistsAsync(int offerId, string userId);
}