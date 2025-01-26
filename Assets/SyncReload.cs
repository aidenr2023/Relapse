using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncReload : MonoBehaviour
{
    //get the animator of the player body
    [SerializeField] private Animator _player_animator;

    public void SyncReloadWithPlayer()
    {

        _player_animator.SetTrigger("Reload");
    }
}