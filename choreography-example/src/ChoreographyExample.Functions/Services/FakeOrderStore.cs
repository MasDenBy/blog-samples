using ChoreographyExample.Functions.Models;

namespace ChoreographyExample.Functions.Services;

public class FakeOrderStore
{
    private readonly List<OrderCreated> _orders = [];

    public void Add(OrderCreated order) => _orders.Add(order);

    public IReadOnlyList<OrderCreated> GetAll() => _orders.AsReadOnly();
}
