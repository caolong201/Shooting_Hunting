using CrazyGames;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrazyGamesLifecycle : MonoBehaviour
{
    private static CrazyGamesLifecycle instance;

    public static void EnsureExists()
    {
        if (instance != null)
            return;

        var lifecycleObject = new GameObject(nameof(CrazyGamesLifecycle));
        DontDestroyOnLoad(lifecycleObject);
        instance = lifecycleObject.AddComponent<CrazyGamesLifecycle>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this)
            instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!CrazySDK.IsInitialized)
            return;

        if (scene.name == "InGame")
            CrazySDK.Game.GameplayStart();
        else
            CrazySDK.Game.GameplayStop();
    }
}
