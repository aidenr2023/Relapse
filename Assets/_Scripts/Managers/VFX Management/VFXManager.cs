using System;
using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private VisualEffect enemyHitEffect;

    #endregion

    #region Private Fields



    #endregion

    private void Awake()
    {
        Instance = this;
    }

}