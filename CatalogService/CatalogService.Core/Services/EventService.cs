using CatalogService.Core.Interfaces;
using CatalogService.Models;

namespace CatalogService.Core.Services;

public class EventService{
    private readonly IEventRepository _repository;

    public EventService(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Event>> GetAllEvents()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Event?> GetEventById(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}