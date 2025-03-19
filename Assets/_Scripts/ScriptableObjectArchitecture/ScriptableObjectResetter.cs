using UnityEngine;

public class ScriptableObjectResetter : MonoBehaviour
{
    [SerializeField] private ResetableScriptableObject scriptableObject;

    public void ResetScriptableObject()
    {
        scriptableObject.Reset();
    }
}