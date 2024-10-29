using System;
using System.Text;
using UnityEngine;

public class SoundManager : MonoBehaviour, IDebugged
{
    #region Constants

    private const int AUDIO_SOURCE_COUNT = 16;

    private const int WORLD_SFX_SOURCE_COUNT = AUDIO_SOURCE_COUNT * 12 / 16;
    // private const int WORLD_SFX_SOURCE_COUNT = AUDIO_SOURCE_COUNT * 4 / 16;
    private const int WORLD_MUSIC_SOURCE_COUNT = 1;
    private const int GLOBAL_MUSIC_SOURCE_COUNT = 1;

    private const int GLOBAL_SFX_SOURCE_COUNT = AUDIO_SOURCE_COUNT - WORLD_SFX_SOURCE_COUNT -
                                                WORLD_MUSIC_SOURCE_COUNT - GLOBAL_MUSIC_SOURCE_COUNT;

    #endregion

    public static SoundManager Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private Transform sourceParent;

    #endregion

    #region Private Fields

    /// <summary>
    /// Audio sources for music that DO NOT MOVE.
    /// </summary>
    private ManagedAudioSource[] _globalMusicSources;

    /// <summary>
    /// Audio sources for music that DO MOVE.
    /// </summary>
    private ManagedAudioSource[] _worldMusicSources;

    /// <summary>
    /// Audio sources for sound effects that DO NOT MOVE.
    /// </summary>
    private ManagedAudioSource[] _globalSfxSources;

    /// <summary>
    /// Audio sources for sound effects that DO MOVE.
    /// </summary>
    private ManagedAudioSource[] _worldSfxSources;

    #endregion

    #region Getters

    // public AudioSource MusicSource => musicSource;
    //
    // public AudioSource SfxSource => sfxSource;

    #endregion

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;

        // Initialize the sources
        InitializeSources();
    }

    private ManagedAudioSource CreateNewManagedSource(string sourceType, int i)
    {
        // Create a new game object for the source
        var newChild = new GameObject($"{sourceType} {i}");

        // Hide the source in the hierarchy
        newChild.transform.hideFlags = HideFlags.HideInHierarchy;

        // Add an audio source to the game object
        var source = newChild.AddComponent<AudioSource>();

        // Set the source to not play on awake
        source.playOnAwake = false;

        // Return the managed audio source
        return new ManagedAudioSource(source);
    }

    private void InitializeSources()
    {
        // Unity can only play 32 audio sources at once,
        // so we need to split the audio sources into groups

        // Create the world sfx sources
        _worldSfxSources = new ManagedAudioSource[WORLD_SFX_SOURCE_COUNT];
        for (var i = 0; i < WORLD_SFX_SOURCE_COUNT; i++)
            _worldSfxSources[i] = CreateNewManagedSource("World SFX", i);

        // Create the global sfx sources
        _globalSfxSources = new ManagedAudioSource[GLOBAL_SFX_SOURCE_COUNT];
        for (var i = 0; i < GLOBAL_SFX_SOURCE_COUNT; i++)
            _globalSfxSources[i] = CreateNewManagedSource("Global SFX", i);

        // Create the world music sources
        _worldMusicSources = new ManagedAudioSource[WORLD_MUSIC_SOURCE_COUNT];
        for (var i = 0; i < WORLD_MUSIC_SOURCE_COUNT; i++)
            _worldMusicSources[i] = CreateNewManagedSource("World Music", i);

        // Create the global music sources
        _globalMusicSources = new ManagedAudioSource[GLOBAL_MUSIC_SOURCE_COUNT];
        for (var i = 0; i < GLOBAL_MUSIC_SOURCE_COUNT; i++)
            _globalMusicSources[i] = CreateNewManagedSource("Global Music", i);
    }

    private void Start()
    {
        // Add this item to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);
    }

    private ManagedAudioSource GetNextSoundSource(ManagedAudioSource[] sources)
    {
        var highestCompletionTime = 0f;
        var highestCompletionIndex = -1;

        for (var i = 0; i < sources.Length; i++)
        {
            // If the current source is not playing, then it is available
            if (!sources[i].Source.isPlaying)
                return sources[i];

            // Skip the current audio source if its sound is persistent
            if (sources[i].CurrentSound.IsPersistent)
                continue;

            // If the current source is playing,
            // check if it's the lowest completion percent
            var completionTime = sources[i].Source.time;

            if (completionTime > highestCompletionTime)
            {
                highestCompletionTime = completionTime;
                highestCompletionIndex = i;
            }
        }

        // Return the source with the lowest completion percent if it exists
        if (highestCompletionIndex >= 0)
            return sources[highestCompletionIndex];

        // If no source is available,
        // return the furthest along source
        highestCompletionTime = 0;
        highestCompletionIndex = 0;

        for (var i = 0; i < sources.Length; i++)
        {
            // If the current source is playing,
            // check if it's the lowest completion percent
            var completionTime = sources[i].Source.time;

            if (completionTime > highestCompletionTime)
            {
                highestCompletionTime = completionTime;
                highestCompletionIndex = i;
            }
        }

        return sources[highestCompletionIndex];
    }

    private void PlaySound(Sound sound, ManagedAudioSource source, bool globalLocation = false)
    {
        // Play the sound
        source?.PlaySound(sound, globalLocation);
    }

    public void PlaySoundAtPoint(Sound sound, ManagedAudioSource source, Vector3 pos)
    {
        // return if the source is null
        if (source == null)
            return;

        // Move the source to the position
        source.MoveToPosition(pos);

        // Play the sound
        PlaySound(sound, source);
    }

    public void PlayMusic(Sound sound) =>
        PlaySound(sound, GetNextSoundSource(_globalMusicSources), true);

    public void PlayMusicAtPoint(Sound sound, Vector3 pos) =>
        PlaySoundAtPoint(sound, GetNextSoundSource(_worldMusicSources), pos);

    public void PlaySfx(Sound sound) =>
        PlaySound(sound, GetNextSoundSource(_globalSfxSources), true);

    public void PlaySfxAtPoint(Sound sound, Vector3 pos) =>
        PlaySoundAtPoint(sound, GetNextSoundSource(_worldSfxSources), pos);

    public string GetDebugText()
    {
        var sb = new StringBuilder();

        sb.Append("Sound Manager\n");

        sb.Append("\tGlobal Music Sources:\n");
        foreach (var source in _globalMusicSources)
            sb.Append($"\t\tSource: {source.GetDebugString()}\n");

        sb.Append("\tWorld Music Sources:\n");
        foreach (var source in _worldMusicSources)
            sb.Append($"\t\tSource: {source.GetDebugString()}\n");

        sb.Append("\tGlobal Sfx Sources:\n");
        foreach (var source in _globalSfxSources)
            sb.Append($"\t\tSource: {source.GetDebugString()}\n");

        sb.Append("\tWorld Sfx Sources:\n");
        foreach (var source in _worldSfxSources)
            sb.Append($"\t\tSource: {source.GetDebugString()}\n");

        return sb.ToString();
    }
}