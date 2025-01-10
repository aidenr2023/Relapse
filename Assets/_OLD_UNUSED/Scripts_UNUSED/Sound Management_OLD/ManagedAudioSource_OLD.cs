using UnityEngine;

public class ManagedAudioSource_OLD
{
    private readonly AudioSource _source;

    private Sound _currentSound;

    #region Getters

    public AudioSource Source => _source;

    public Sound CurrentSound => _currentSound;

    public float CompletionPercent => _source.time / _source.clip.length;

    #endregion

    public ManagedAudioSource_OLD(AudioSource source)
    {
        _source = source;
    }

    private void SetSoundSettings(Sound sound)
    {
        if (sound == null)
            return;

        // Assign the sound clip to the source
        Source.clip = sound.Clip;

        // Assign the sound settings to the Source
        // Source.loop = sound.Settings.Loop;
        Source.volume = sound.Volume;

        // if (sound.Settings == null)
        //     return;
        //
        // Source.pitch = sound.Settings.Pitch;
        // Source.panStereo = sound.Settings.StereoPan;
        // Source.spatialBlend = sound.Settings.SpatialBlend;
        // Source.reverbZoneMix = sound.Settings.ReverbZoneMix;
    }

    public void PlaySound(Sound sound, bool globalLocation = false)
    {
        // Return if the sound is null
        if (sound == null)
            return;

        bool _isPlaying = _source.isPlaying;

        // If the sound is already playing, stop it
        _source.Pause();
        _source.Stop();

        // Debug.Log($"Playing sound: {sound} from source: {_source}");

        // Set the current sound
        _currentSound = sound;

        // Set the sound settings
        SetSoundSettings(sound);

        // Set the source's spatial blend to 0 if the sound is global
        if (globalLocation)
            _source.spatialBlend = 0;

        // Play the sound
        _source.PlayDelayed(0);
    }

    public void MoveToPosition(Vector3 pos)
    {
        _source.transform.position = pos;
    }

    public string GetDebugString()
    {
        return $"Play? {(_source != null ? _source.isPlaying : "FALSE")} " +
               $"Sound: {(_currentSound != null ? _currentSound : "NONE")}";
    }
}