using Microsoft.EntityFrameworkCore;

public class CatalogDbContext: DbContext{
    public DbSet<Event> Events { get; set; }
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options):base(options){

    }
}