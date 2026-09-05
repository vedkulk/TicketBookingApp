namespace CatalogService.Api.Dtos;

public class CreateEventDto{
    public string Name {get; set;}
    public DateTime Date {get; set;}
    public int VenueId {get; set;}
}