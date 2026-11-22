namespace GroupLib.Models
{
    internal sealed class GroupBucket<TItem>(object? key)
    {
        public object? Key { get; } = key;
        public List<TItem> Items { get; } = [];
    }
}
