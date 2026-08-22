using Microsoft.EntityFrameworkCore;
using CatalogService.Models;

namespace CatalogService.Infrastructure;

public class CatalogDbContext: DbContext{
    public DbSet<Event> Events { get; set; }
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options):base(options){

    }
}