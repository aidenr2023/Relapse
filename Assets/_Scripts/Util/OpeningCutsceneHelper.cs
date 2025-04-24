using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OpeningCutsceneHelper : MonoBehaviour
{
    [SerializeField] private EventVariable gameOnStart;
    [SerializeField] private CanvasGroup blackOverlayGroup;
    [SerializeField, Min(0)] private float blackOverlayTransitionTime = .5f;
    [SerializeField] private LevelStartupSceneInfo levelStartupSceneInfo;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private SceneField openingCutscene;

    private bool _showLoadingBar;

    public void StartButton()
    {
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        // Fade into the black overlay
        var startTime = Time.unscaledTime;

        while (Time.unscaledTime - startTime < blackOverlayTransitionTime)
        {
            var time = (Time.unscaledTime - startTime) / blackOverlayTransitionTime;
            blackOverlayGroup.alpha = time;

            yield return null;
        }

        blackOverlayGroup.alpha = 1;

        // Set the show loading bar flag to true
        _showLoadingBar = true;
        loadingBar.gameObject.SetActive(_showLoadingBar);

        // StartCoroutine(LoadSceneAsync());
        AsyncSceneManager.Instance.LoadStartupScene(
            levelStartupSceneInfo, this, UpdateProgressBarPercent,
            // Deactivate
            () => { SceneManager.UnloadSceneAsync(openingCutscene.SceneName); }
        );

        yield return null;

        // Invoke the game on start event
        gameOnStart.Invoke();
    }

    private void UpdateProgressBarPercent(float amount)
    {
        loadingBar.value = amount;
    }
}