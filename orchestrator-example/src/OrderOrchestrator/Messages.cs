namespace OrderOrchestrator;

public class OrderPlaced
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public int Amount { get; set; }
}

public class BookInventoryCommand
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public string Product { get; set; } = string.Empty;
    public int Amount { get; set; }
}

public class BookInventoryResult
{
    public Guid CorrelationId { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
}

public class SendEmailCommand
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
}

public class SendEmailResult
{
    public Guid CorrelationId { get; set; }
    public bool Success { get; set; }
}

public class CreateDeliveryCommand
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
}

public class CreateDeliveryResult
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public Guid DeliveryId { get; set; }
    public bool Success { get; set; }
}
