using Microsoft.AspNetCore.Mvc;
using CatalogService.Core.Services;
using CatalogService.Api.Dtos;
using CatalogService.Models;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController: ControllerBase
{
    private readonly EventService _eventService;

    public EventsController(EventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var events = await _eventService.GetAllEvents();
        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var evnt = await _eventService.GetEventById(id);
        if (evnt == null)
        {
            return NotFound();
        }
        return Ok(evnt);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEventDto dto)
    {
        var newEvent = new Event{
            Name = dto.Name,
            Date = dto.Date,
            Venue = dto.Venue,
        };
        var created = await _eventService.CreateEvent(newEvent); // mapping goes here
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}