using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System;

public class CutsceneManager : MonoBehaviour
{
    #region Singleton
    public static CutsceneManager Instance { get; private set; }
    
    //public PlayerMovementV2 PlayerController { get; private set; }
    
    //public GameObject PlayerGameObject { get; private set; }
    
    //public Animator PlayerAnimator { get; private set; }
    
    //get the player animator from cutscene subscriber
    public Animator PlayerCutsceneAnimator { get; private set; }
    
    #endregion
    
    [System.Serializable]
    public class CutsceneMapping
    {
        public string cutsceneName;
        public PlayableAsset timelineAsset;
    }

    [Header("Configuration")]
    [SerializeField] private List<CutsceneMapping> cutsceneMappings = new List<CutsceneMapping>();
    [SerializeField] private CutsceneHandler cutsceneHandler;
    public CutsceneHandler CutsceneHandler => cutsceneHandler;

    private Dictionary<string, PlayableAsset> cutsceneDictionary;
    private PlayableDirector activeDirector;

    #region Lifecycle
    
    public void RegisterPlayer(Animator playerAnimator)
    {
        PlayerCutsceneAnimator = playerAnimator;
        Debug.Log($"Player registered: {playerAnimator.name} " +
                  $"| Animator: {playerAnimator} != null");
        //player.GetComponent<Animator>();
    }
    private void Awake()
    {
        InitializeSingleton();
        InitializeDictionary();
        CacheActiveDirector();
    }

    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void InitializeDictionary()
    {
        cutsceneDictionary = new Dictionary<string, PlayableAsset>();
        foreach (var mapping in cutsceneMappings)
        {
            if (!cutsceneDictionary.ContainsKey(mapping.cutsceneName))
            {
                cutsceneDictionary.Add(mapping.cutsceneName, mapping.timelineAsset);
            }
            else
            {
                Debug.LogWarning($"Duplicate cutscene name: {mapping.cutsceneName}");
            }
        }
    }

    private void CacheActiveDirector()
    {
        if (cutsceneHandler != null)
        {
            activeDirector = cutsceneHandler.GetComponent<PlayableDirector>();
        }
    }
    #endregion

    #region Public API
    public void PlayCutsceneByName(string cutsceneName, bool isPlayerMovementNeeded, CutsceneHandler.CutsceneType perspective)
    {
        if (!cutsceneDictionary.TryGetValue(cutsceneName, out PlayableAsset asset))
        {
            Debug.LogError($"Cutscene not found: {cutsceneName}");
            
            return;
        }
        cutsceneHandler.PlayCutscene(asset, !isPlayerMovementNeeded, perspective);
        

        if (activeDirector == null)
        {
            Debug.LogError("No active PlayableDirector found!");
            return;
        }
        //log asset name 
        Debug.Log($"Playing cutscene asset: {asset.name}");
        StartCutsceneSequence(asset, perspective);
    }
    #endregion

    #region Execution
    /// <summary>
    /// gets the timeline asset and perspective-> plays the cutscene
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="perspective"></param>
    private void StartCutsceneSequence(PlayableAsset asset, CutsceneHandler.CutsceneType perspective)
    {
        try
        {
           cutsceneHandler.PlayCutscene(asset, cutsceneHandler.IsPlayerMovementNeeded, 
           perspective);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Cutscene playback failed: {e.Message}");
        }
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    #endregion
}