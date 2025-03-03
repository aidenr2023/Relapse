using System;
using System.Collections.Generic;

public class MemoryManager
{
    private static MemoryManager _instance;

    public static MemoryManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new MemoryManager();

            return _instance;
        }
    }

    private readonly List<MemoryScriptableObject> _memories = new();

    public event Action<MemoryScriptableObject> OnMemoryAdded;

    public IReadOnlyCollection<MemoryScriptableObject> Memories => _memories;

    private MemoryManager()
    {
        // Add to the memory added event
        OnMemoryAdded += TooltipOnMemoryAdded;
    }

    private void TooltipOnMemoryAdded(MemoryScriptableObject memory)
    {
        // Return if the memory is null
        if (memory == null)
            return;
        
        JournalTooltipManager.Instance.AddTooltip($"Memory Added: {memory.MemoryName}\nCheck the Journal For Details!");
    }

    public void AddMemory(MemoryScriptableObject memory)
    {
        // Return if the memory is already in the list
        if (_memories.Contains(memory))
            return;

        // Add the memory to the list
        _memories.Add(memory);

        // Invoke the event
        OnMemoryAdded?.Invoke(memory);
    }
}