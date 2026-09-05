using Microsoft.AspNetCore.Mvc;
using CatalogService.Core.Services;
using CatalogService.Api.Dtos;
using CatalogService.Models;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VenuesController: ControllerBase
{
    private readonly VenueService _VenueService;

    public VenuesController(VenueService VenueService)
    {
        _VenueService = VenueService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var Venues = await _VenueService.GetAllVenues();
        return Ok(Venues);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var evnt = await _VenueService.GetVenueById(id);
        if (evnt == null)
        {
            return NotFound();
        }
        return Ok(evnt);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVenueDto dto)
    {
        var newVenue = new Venue{
            Name = dto.Name,
            Address = dto.Address
        };
        var created = await _VenueService.CreateVenue(newVenue); // mapping goes here
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}