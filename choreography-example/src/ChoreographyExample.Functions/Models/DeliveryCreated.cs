namespace ChoreographyExample.Functions.Models;

public class DeliveryCreated
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public Guid DeliveryId { get; set; }
}
