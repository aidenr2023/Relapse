using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuContentPane : MonoBehaviour
{
    [SerializeField] private GameObject firstItem;
    [SerializeField] private GameObject lastItem;

    public GameObject FirstItem => firstItem;
    public GameObject LastItem => lastItem;

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public void SetUpDownNavigation(Selectable selectable)
    {
        // Return if the first item is null
        if (firstItem == null)
            return;

        var firstItemSelectable = firstItem.GetComponent<Selectable>();

        if (firstItemSelectable == null)
            Debug.LogError($"{firstItem.name} does not have a Selectable component attached to it!");

        if (firstItem == null || firstItemSelectable == null || selectable == null)
            return;

        // Set the new navigation
        var oldFirstNavigation = firstItemSelectable.navigation;
        firstItemSelectable.navigation = new Navigation()
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = selectable,
            selectOnDown = oldFirstNavigation.selectOnDown,
        };

        // Change the down navigation of the selectable to the first item
        var oldSelectableNavigation = selectable.navigation;
        selectable.navigation = new Navigation()
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = oldSelectableNavigation.selectOnUp,
            selectOnDown = firstItemSelectable,
            selectOnLeft = oldSelectableNavigation.selectOnLeft,
            selectOnRight = oldSelectableNavigation.selectOnRight,
        };
    }
}