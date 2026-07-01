namespace ChoreographyExample.Functions.Services;

public class FakeInventoryStore
{
    private readonly Dictionary<string, int> _inventory = new()
    {
        { "Laptop", 10 },
        { "Mouse", 50 },
        { "Keyboard", 30 },
        { "Monitor", 15 },
        { "Headphones", 25 }
    };

    public bool TryBook(string product, int amount)
    {
        if (!_inventory.TryGetValue(product, out var available) || available < amount)
            return false;

        _inventory[product] -= amount;
        return true;
    }

    public IReadOnlyDictionary<string, int> GetAll() => _inventory.AsReadOnly();
}
