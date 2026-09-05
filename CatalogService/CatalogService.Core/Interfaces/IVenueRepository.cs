using CatalogService.Models;
namespace CatalogService.Core.Interfaces;

public interface IVenueRepository{
    Task<List<Venue>> GetAllAsync();
    Task<Venue?> GetByIdAsync(int id);
    Task<Venue> AddAsync(Venue newVenue);
}