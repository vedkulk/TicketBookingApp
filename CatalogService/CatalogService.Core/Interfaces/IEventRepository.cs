using CatalogService.Models;
namespace CatalogService.Core.Interfaces;

public interface IEventRepository{
    Task<List<Event>> GetAllAsync();
    Task<Event?> GetByIdAsync(int id);
}