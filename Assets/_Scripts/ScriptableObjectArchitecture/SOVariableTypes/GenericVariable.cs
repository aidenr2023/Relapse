using UnityEngine;

public abstract class GenericVariable<T> : ScriptableObject
{
    public const string VARIABLES_PATH = "Scriptable Object Architecture/Variables";
    
    public T value;
}