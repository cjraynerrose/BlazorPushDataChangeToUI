using Serilog;

namespace UpdatingTable.Data
{
    public class DataService
    {
        private static readonly Dictionary<int, Item> _items = new()
        {
#if DEBUG   
            {0, new Item() {Id = 0, Name = "Item 0", Description = "The zeroth item" } },
            {1, new Item() {Id = 1, Name = "Item 1", Description = "The first item" } },
            {2, new Item() {Id = 2, Name = "Item 2", Description = "The second item" } },
            {3, new Item() {Id = 3, Name = "Item 3", Description = "The third item" } },
            {4, new Item() {Id = 4, Name = "Item 4", Description = "The fourth item" } },
            {5, new Item() {Id = 5, Name = "Item 5", Description = "The fifth item" } }  
#endif
        };

        private static int _nextKey = _items?.Count ?? 0;

        private readonly ILogger _logger;

        public event EventHandler<ItemChangedEvent>? ItemChanged;

        public DataService(
            ILogger logger)
        {
            _logger = logger;
        }

        public void Add(Item item)
        {
            var key = _nextKey++;
            item.Id = key;
            _items.Add(key, item);
            _logger.Information("Added: {Id}", key);

            ItemChanged?.Invoke(this, new(item, State.Added));
        }

        public void Remove(Item item)
        {
            var keyFound = _items.Remove(item.Id);

            if (keyFound)
            {
                _logger.Information("Removed: {Id}", item.Id);
                ItemChanged?.Invoke(this, new(item, State.Deleted));
            }
            else
            {
                _logger.Information("Key not found to remove: {Id}", item.Id);
            }
        }

        public void Update(Item item)
        {
            if (!_items.ContainsKey(item.Id))
            {
                _logger.Information("Key not found to update: {Id}", item.Id);
                return;
            }

            _items[item.Id] = item;
            _logger.Information("Updated: {Id}", item.Id);
            ItemChanged?.Invoke(this, new(item, State.Updated));
        }

        public Item? Get(int id)
        {
            if (!_items.TryGetValue(id, out Item? value))
            {
                _logger.Information("Key not found to get: {Id}", id);
                return null;
            }

            _logger.Information("Getting: {Id}", id);
            return value;
        }

        public List<Item> Get()
        {
            _logger.Information("Getting {Count} items", _items.Count);
            return [.. _items.Values];
        }
    }

    public struct ItemChangedEvent
    {
        public ItemChangedEvent(Item item, State state)
        {
            Item = item;
            NewState = state;
        }

        public Item Item;
        public State NewState;
    }

    public enum State
    {
        Added,
        Updated,
        Deleted
    }
}
