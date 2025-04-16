using UnityEngine;
using System.Collections;

public class MusicPlaylistWithFading : MonoBehaviour
{
    [Header("Playlist Settings")]
    public AudioClip[] tracks;      // Array of music tracks.
    public bool shuffle = false;    // If true, selects a random track.

    [Header("Fade Settings")]
    public float fadeDuration = 2f; // Duration (in seconds) of the fade in and fade out.

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float volume = 1f;       // Overall desired volume level.

    private AudioSource audioSource;
    private int currentTrack = 0;

    void Awake()
    {
        // Get the pre-existing AudioSource.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("MusicPlaylistWithFading requires an AudioSource on the same GameObject.");
            return;
        }

        // Disable native looping; our script manages track transitions.
        audioSource.loop = false;
        audioSource.volume = volume;
    }

    void Start()
    {
        if (tracks == null || tracks.Length == 0)
        {
            Debug.LogWarning("No tracks assigned to MusicPlaylistWithFading.");
            return;
        }
        StartCoroutine(PlayPlaylist());
    }

    IEnumerator PlayPlaylist()
    {
        while (true)
        {
            // Select the next track (sequential or random).
            if (shuffle)
                currentTrack = Random.Range(0, tracks.Length);
            else
                currentTrack %= tracks.Length;

            // Set the clip and start playback with volume at 0.
            audioSource.clip = tracks[currentTrack];
            audioSource.volume = 0f;
            audioSource.Play();

            float clipLength = audioSource.clip.length;

            // Use fades only if the track is long enough.
            if (clipLength > 2 * fadeDuration)
            {
                // Fade in.
                float timer = 0f;
                while (timer < fadeDuration)
                {
                    audioSource.volume = Mathf.Lerp(0f, volume, timer / fadeDuration);
                    timer += Time.deltaTime;
                    yield return null;
                }
                audioSource.volume = volume;

                // Wait until it's time to start fading out.
                yield return new WaitForSeconds(clipLength - 2 * fadeDuration);

                // Fade out.
                timer = 0f;
                while (timer < fadeDuration)
                {
                    audioSource.volume = Mathf.Lerp(volume, 0f, timer / fadeDuration);
                    timer += Time.deltaTime;
                    yield return null;
                }
                audioSource.volume = 0f;

                // Ensure the track is finished.
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                // If the track is too short for fades, just play it in full.
                audioSource.volume = volume;
                yield return new WaitForSeconds(clipLength);
            }

            // Move on to the next track.
            currentTrack++;
        }
    }
}
