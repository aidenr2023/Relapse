using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CustomStack<TItemType> : IEnumerable<TItemType>
{
    private readonly SortedList<ulong, TItemType> _sortedList = new();

    private TItemType _topItem;

    private ulong _counter = 0;

    public CustomStack(params TItemType[] items)
    {
        foreach (var item in items)
            Push(item);
    }

    public void Push(TItemType item)
    {
        // Add the item to the list
        _sortedList.Add(_counter, item);

        // Set the top item
        _topItem = item;

        // Increment the counter
        _counter++;
    }

    public TItemType Pop()
    {
        // Get the last item
        var item = _topItem;

        // Remove the last item
        _sortedList.RemoveAt(_sortedList.Count - 1);

        // Set the top item
        _topItem = _sortedList.Last().Value;

        // Decrement the counter
        _counter--;

        return item;
    }

    public TItemType Peek()
    {
        return _topItem;
    }

    public void Clear()
    {
        // Clear the list
        _sortedList.Clear();

        // Clear the top item
        _topItem = default;

        // Reset the counter
        _counter = 0;
    }

    public void Remove(TItemType item)
    {
        // Find the item in the list
        var index = _sortedList.IndexOfValue(item);

        // Return if the item is not in the list
        if (index == -1)
            return;

        // Remove the item
        _sortedList.RemoveAt(index);

        // If there are no items in the list, return
        if (_sortedList.Count == 0)
        {
            _topItem = default;
            return;
        }

        // Set the top item
        _topItem = _sortedList.Last().Value;
    }

    public IEnumerator<TItemType> GetEnumerator()
    {
        foreach (var item in _sortedList)
            yield return item.Value;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return (IEnumerator<TItemType>)_sortedList.GetEnumerator();
    }
}