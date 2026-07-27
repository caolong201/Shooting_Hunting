using CrazyGames;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoadingBootstrap : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Home";

    private void Start()
    {
        EnsureAdManager();

        if (CrazySDK.IsAvailable)
        {
            CrazySDK.Init(OnSdkReady);
        }
        else
        {
            LoadNextScene();
        }
    }

    private void OnSdkReady()
    {
        EnsureAdManager();
        CrazyGamesLifecycle.EnsureExists();
        CrazySDK.Ad.PrefetchAd(CrazyAdType.Rewarded);
        LoadNextScene();
    }

    private static void EnsureAdManager()
    {
        if (AdManager.IsInstanceValid())
            return;

        var adManagerObject = new GameObject("AdManager");
        DontDestroyOnLoad(adManagerObject);
        adManagerObject.AddComponent<AdManager>();
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}
