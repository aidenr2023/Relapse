using System.Collections.Generic;

public class DebugManager : IDebugged
{
    #region Singleton Pattern

    private static DebugManager _instance;

    public static DebugManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new DebugManager();

            return _instance;
        }
    }

    #endregion

    private readonly HashSet<IDebugged> _debuggedObjects = new();

    public bool IsDebugMode { get; set; }

    public IReadOnlyCollection<IDebugged> DebuggedObjects => _debuggedObjects;


    private DebugManager()
    {
        // Add this to the debug managed objects
        AddDebuggedObject(this);
    }

    public void AddDebuggedObject(IDebugged debugged)
    {
        // Add the debug managed object to the hash set
        _debuggedObjects.Add(debugged);
    }

    public void RemoveDebuggedObject(IDebugged debugged)
    {
        // Remove the debug managed object from the hash set
        _debuggedObjects.Remove(debugged);
    }


    public string GetDebugText()
    {
        return "PRESS F1 TO TOGGLE DEBUG MODE\n";
    }
}