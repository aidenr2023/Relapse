using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SoundPool
{
    [SerializeField] private Sound[] sounds;

    public IReadOnlyCollection<Sound> Sounds => sounds;

    public Sound GetRandomSound()
    {
        // Avoid null reference exception
        if (sounds == null || sounds.Length == 0)
            return null;

        // Get the index of the random sound
        var randomIndex = UnityEngine.Random.Range(0, sounds.Length);

        return sounds[randomIndex];
    }
}