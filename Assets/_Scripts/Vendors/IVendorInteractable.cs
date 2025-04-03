using UnityEngine.Events;

public interface IVendorInteractable : IInteractable
{
    public void InvokeAfterTalkingOnce();
}