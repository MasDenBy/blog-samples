namespace ChoreographyExample.Functions.Models;

public class OrderCreated
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public int Amount { get; set; }
}
