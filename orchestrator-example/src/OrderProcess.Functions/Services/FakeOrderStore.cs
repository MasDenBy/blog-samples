using OrderProcess.Functions.Models;

namespace OrderProcess.Functions.Services;

public class FakeOrderStore
{
    private readonly List<OrderCreated> _orders = [];

    public void Add(OrderCreated order) => _orders.Add(order);

    public IReadOnlyList<OrderCreated> GetAll() => _orders.AsReadOnly();
}
