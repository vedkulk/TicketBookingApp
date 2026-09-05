namespace CatalogService.Models;

public class Event{
    public int Id { get; set; }
    public string Name {get; set;}
    public DateTime Date {get; set;}
    public int VenueId {get; set;}
    public Venue Venue {get; set;}
}