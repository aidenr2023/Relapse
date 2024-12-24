using System;
using UnityEngine;

public class MenuManagerHelper : MonoBehaviour
{
    private void Update()
    {
        MenuManager.Instance.Update();
    }
}