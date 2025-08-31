using AutoLease.Domain.Entities;

namespace AutoLease.Domain.Interfaces;

public interface IOfferRepository
{
    Task<Offer?> GetByIdAsync(int id);
    Task<IEnumerable<Offer>> GetAllAsync();
    Task<IEnumerable<Offer>> GetActiveOffersAsync();
    Task<IEnumerable<Offer>> GetOffersByCarIdAsync(int carId);
    Task<IEnumerable<Offer>> GetOffersBySalesAgentIdAsync(int salesAgentId);
    Task<IEnumerable<Offer>> GetFilteredOffersAsync(string? searchTerm, decimal? minDiscount, decimal? maxPrice, bool? isActive, int pageNumber, int pageSize);
    Task<int> GetTotalCountAsync(string? searchTerm, decimal? minDiscount, decimal? maxPrice, bool? isActive);
    Task<Offer> AddAsync(Offer offer);
    Task UpdateAsync(Offer offer);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}