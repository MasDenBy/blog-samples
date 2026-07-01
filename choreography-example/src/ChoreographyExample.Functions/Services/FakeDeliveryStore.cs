using ChoreographyExample.Functions.Models;

namespace ChoreographyExample.Functions.Services;

public class FakeDeliveryStore
{
    private readonly List<DeliveryCreated> _deliveries = [];

    public void Add(DeliveryCreated delivery) => _deliveries.Add(delivery);

    public IReadOnlyList<DeliveryCreated> GetAll() => _deliveries.AsReadOnly();
}
