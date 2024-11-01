using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private TMP_Text text;

    #endregion

    #region Getters

    public TMP_Text Text => text;

    #endregion
}