using UnityEngine;

public class TmpSoundEmitter : MonoBehaviour
{
    [SerializeField] private SoundPool sounds;

    [SerializeField] private CountdownTimer timeBetweenSounds = new(1f, true);

    // Start is called before the first frame update
    private void Start()
    {
        timeBetweenSounds.OnTimerEnd += () =>
        {
            // SoundManager_OLD.Instance.PlaySfxAtPoint(sounds.GetRandomSound(), transform.position);
            SoundManager.Instance.PlaySfxAtPoint(sounds.GetRandomSound(), transform.position);
            timeBetweenSounds.Reset();
        };
    }

    // Update is called once per frame
    private void Update()
    {
        // Update the timer
        timeBetweenSounds.Update(Time.deltaTime);
    }
}