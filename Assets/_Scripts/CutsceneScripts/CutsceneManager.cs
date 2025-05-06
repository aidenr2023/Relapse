using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System;

public class CutsceneManager : MonoBehaviour
{
    #region Singleton

    public static CutsceneManager Instance { get; private set; }

    #endregion

    #region Serialized Fields

    [Header("Configuration"), SerializeField]
    private List<CutsceneMapping> cutsceneMappings = new();

    [SerializeField] private CutsceneHandler cutsceneHandler;

    #endregion

    #region Private Fields

    private Dictionary<string, PlayableAsset> _cutsceneDictionary;
    private PlayableDirector _activeDirector;

    #endregion

    //get the player animator from cutscene subscriber
    public Animator PlayerCutsceneAnimator { get; private set; }
    
    public CutsceneHandler CutsceneHandler => cutsceneHandler;

    #region Lifecycle

    public void RegisterPlayer(Animator playerAnimator)
    {
        PlayerCutsceneAnimator = playerAnimator;
        Debug.Log($"Player registered: {playerAnimator.name} | Animator: {playerAnimator} != null");
    }

    private void Awake()
    {
        InitializeDictionary();
        CacheActiveDirector();

        InitializeSingleton();
    }

    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            // Transfer all the information on this cutscene handler to the existing instance
            CopyToOtherInstance(Instance);
            
            // Destroy this instance
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void CopyToOtherInstance(CutsceneManager otherInstance)
    {
        Debug.Log($"Copying CutsceneManager to other instance: {otherInstance.name}", this);
        
        // Register the player animator if it exists
        if (PlayerCutsceneAnimator != null)
            otherInstance.RegisterPlayer(PlayerCutsceneAnimator);
            
        // Copy over the cutscene mappings
        otherInstance.cutsceneMappings = cutsceneMappings;

        // Copy over the cutscene dictionary
        otherInstance._cutsceneDictionary = _cutsceneDictionary;
        
        // Reinitialize the cutscene handler
        otherInstance.cutsceneHandler._cmBrain = cutsceneHandler._cmBrain;
        otherInstance.cutsceneHandler.Initialize();
    }

    private void InitializeDictionary()
    {
        _cutsceneDictionary = new Dictionary<string, PlayableAsset>();
        foreach (var mapping in cutsceneMappings)
        {
            if (!_cutsceneDictionary.ContainsKey(mapping.cutsceneName))
                _cutsceneDictionary.Add(mapping.cutsceneName, mapping.timelineAsset);
            else
                Debug.LogWarning($"Duplicate cutscene name: {mapping.cutsceneName}");
        }
    }

    private void CacheActiveDirector()
    {
        if (cutsceneHandler != null)
            _activeDirector = cutsceneHandler.GetComponent<PlayableDirector>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    #endregion

    #region Public API

    public void PlayCutsceneByName(string cutsceneName, bool isPlayerMovementNeeded,
        CutsceneHandler.CutsceneType perspective)
    {
        if (!_cutsceneDictionary.TryGetValue(cutsceneName, out PlayableAsset asset))
        {
            Debug.LogError($"Cutscene not found: {cutsceneName}");
            return;
        }

        cutsceneHandler.PlayCutscene(asset, !isPlayerMovementNeeded, perspective);


        if (_activeDirector == null)
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
            cutsceneHandler.PlayCutscene(asset, cutsceneHandler.IsPlayerMovementNeeded, perspective);
        }
        catch (Exception e)
        {
            Debug.LogError($"Cutscene playback failed: {e.Message}");
        }
    }

    #endregion

    [Serializable]
    public class CutsceneMapping
    {
        public string cutsceneName;
        public PlayableAsset timelineAsset;
    }
}