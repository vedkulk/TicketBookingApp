using CatalogService.Core.Interfaces;
using CatalogService.Models;
namespace CatalogService.Core.Services;

public class VenueService{
    private readonly IVenueRepository _repository;

    public VenueService(IVenueRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Venue>> GetAllVenues()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Venue?> GetVenueById(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Venue> CreateVenue(Venue newVenue)
    {
        return await _repository.AddAsync(newVenue);
    }
}