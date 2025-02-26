using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Hoversound : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]private AudioClip hoverSound;
    [SerializeField]private AudioSource source;
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Play the hover sound
        source.PlayOneShot(hoverSound);
        Debug.Log("Mouse entered");
        
    }
}


