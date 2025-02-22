using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

/// <summary>
/// ScriptableObject that holds cutscene data, including the timeline asset and a virtual camera reference.
/// </summary>
[CreateAssetMenu(fileName = "NewCutsceneData", menuName = "Cutscene/CutsceneData")]
public class CutsceneData : MonoBehaviour
{
    [Header("Cutscene Data")]
    [SerializeField] private string cutsceneName;
    //[SerializeField] private PlayableAsset timelineAsset;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    public string CutsceneName => cutsceneName;
    //public PlayableAsset TimelineAsset => timelineAsset;
    public CinemachineVirtualCamera VirtualCamera => virtualCamera;

    private bool cutscenePlayed = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !cutscenePlayed)
        {
            CutsceneManager.Instance.PlayCutsceneByName(cutsceneName);
            cutscenePlayed = true;
        }
    }
}