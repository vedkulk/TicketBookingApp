using Microsoft.EntityFrameworkCore;
using CatalogService.Core.Interfaces;
using CatalogService.Core.Services;
using CatalogService.Infrastructure.Repositories;
using CatalogService.Infrastructure; // still needed for CatalogDbContext itself

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDbConnection")));
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<EventService>();

var app = builder.Build();

if(app.Environment.IsDevelopment()){
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
