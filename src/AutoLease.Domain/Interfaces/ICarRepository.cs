using AutoLease.Domain.Entities;

namespace AutoLease.Domain.Interfaces;

public interface ICarRepository
{
    Task<Car?> GetByIdAsync(int id);
    Task<Car?> GetByVinNumberAsync(string vinNumber);
    Task<IEnumerable<Car>> GetAllAsync();
    Task<IEnumerable<Car>> GetAvailableCarsAsync();
    Task<IEnumerable<Car>> GetCarsByOwnerIdAsync(string ownerId);
    Task<IEnumerable<Car>> GetCarsBySalesAgentIdAsync(int salesAgentId);
    Task<IEnumerable<Car>> GetFilteredCarsAsync(string? searchTerm, string? make, int? year, decimal? minPrice, decimal? maxPrice, bool? isAvailable, int? salesAgentId, int pageNumber, int pageSize, string sortBy, bool sortDescending);
    Task<int> GetTotalCountAsync(string? searchTerm, string? make, int? year, decimal? minPrice, decimal? maxPrice, bool? isAvailable, int? salesAgentId);
    Task<Car> AddAsync(Car car);
    Task UpdateAsync(Car car);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> VinNumberExistsAsync(string vinNumber);
}