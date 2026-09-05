using CatalogService.Models;
using CatalogService.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public class VenueRepository : IVenueRepository{
    private readonly CatalogDbContext _context;

    public VenueRepository(CatalogDbContext context){
        _context = context;
    }

     public async Task<List<Venue>> GetAllAsync(){
        return await _context.Venues.ToListAsync();
    }

    public async Task<Venue?> GetByIdAsync(int id)
    {
        return await _context.Venues.FindAsync(id);
    }

    public async Task<Venue> AddAsync(Venue newVenue){
        _context.Venues.Add(newVenue);
        await _context.SaveChangesAsync();
        return newVenue;
    }
}