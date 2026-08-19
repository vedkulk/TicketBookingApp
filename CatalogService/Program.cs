using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDbConnection")));

var app = builder.Build();

if(app.Environment.IsDevelopment()){
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!");
app.MapGet("/events", async (CatalogDbContext context) => await context.Events.ToListAsync());
app.MapGet("/events/{id}", async (int id, CatalogDbContext context) =>
{
    var evnt = await context.Events.FindAsync(id);

    if (evnt == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(evnt);
});
app.Run();
