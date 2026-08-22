using CatalogService.Models;
using CatalogService.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public class EventRepository : IEventRepository{
    private readonly CatalogDbContext _context;

    public EventRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<List<Event>> GetAllAsync(){
        return await _context.Events.ToListAsync();
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        return await _context.Events.FindAsync(id);
    }
}