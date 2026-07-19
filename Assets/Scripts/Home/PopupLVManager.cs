using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupLVManager : MonoBehaviour
{
    public const string PlayStageNoKey = "PlayStageNo";

    private const int TotalLevels = 10;
    private const string StageNoKey = "StageNo";

    [SerializeField] private GameObject popupLVTemplate;
    [SerializeField] private Transform contentParent;
    [SerializeField] private EnemyPreviewManager enemyPreviewManager;
    [SerializeField] private Camera previewCamera;
    [SerializeField] private RenderTexture previewRenderTexture;
    [SerializeField] private Texture[] animalTextures;
    [SerializeField] private LifePopup lifePopup;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject levelPlayPanel;
    [SerializeField] private GameObject startHuntingButton;

    private readonly List<LevelPopupUI> levelPopups = new List<LevelPopupUI>();
    private readonly List<RenderTexture> capturedTextures = new List<RenderTexture>();
    private GameObject tempPreviewObject;
    private bool initialized;

    private void Start()
    {
        ResolveReferences();
        BindBackButton();
        StartCoroutine(InitializePopups());
    }

    private void OnEnable()
    {
        if (initialized)
            RefreshAll();
    }

    private void ResolveReferences()
    {
        if (contentParent == null)
            contentParent = transform;

        if (popupLVTemplate == null && contentParent.childCount > 0)
            popupLVTemplate = contentParent.GetChild(0).gameObject;

        if (enemyPreviewManager == null)
            enemyPreviewManager = FindObjectOfType<EnemyPreviewManager>();

        if (previewCamera == null)
        {
            foreach (Camera cam in FindObjectsOfType<Camera>())
            {
                if (cam.targetTexture == null)
                    continue;

                previewCamera = cam;
                if (previewRenderTexture == null)
                    previewRenderTexture = cam.targetTexture as RenderTexture;
                break;
            }
        }

        if (lifePopup == null)
            lifePopup = FindObjectOfType<LifePopup>(true);

        if (levelPlayPanel == null)
        {
            Transform current = transform;
            while (current != null)
            {
                if (current.name == "LEVELPLAY")
                {
                    levelPlayPanel = current.gameObject;
                    break;
                }

                current = current.parent;
            }
        }

        if (backButton == null && levelPlayPanel != null)
        {
            Transform backTransform = FindDeepChild(levelPlayPanel.transform, "BackBnt");
            if (backTransform != null)
                backButton = backTransform.gameObject;
        }
    }

    private void BindBackButton()
    {
        if (backButton == null)
            return;

        Button button = backButton.GetComponent<Button>();
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnBackClicked);
    }

    public void OnBackClicked()
    {
        if (levelPlayPanel != null)
            levelPlayPanel.SetActive(false);

        if (startHuntingButton != null)
            startHuntingButton.SetActive(true);
    }

    private IEnumerator InitializePopups()
    {
        if (popupLVTemplate == null)
        {
            Debug.LogWarning("PopupLVManager: popupLVTemplate is not assigned.");
            yield break;
        }

        popupLVTemplate.SetActive(false);
        BuildLevelPopups();
        RefreshAll();

        yield return null;

        SetSpawnPointChildrenActive(false);
        ClearTempPreview();

        for (int i = 0; i < levelPopups.Count; i++)
        {
            Texture texture = GetAnimalTexture(i);
            if (texture != null)
                levelPopups[i].AnimalImage.texture = texture;

            yield return null;
        }

        ClearTempPreview();
        RestoreMainEnemyPreview();
        initialized = true;
    }

    private void OnDestroy()
    {
        ClearTempPreview();

        foreach (RenderTexture texture in capturedTextures)
        {
            if (texture != null)
                texture.Release();
        }

        capturedTextures.Clear();
    }

    private void SetSpawnPointChildrenActive(bool active)
    {
        if (enemyPreviewManager == null || enemyPreviewManager.spawnPoint == null)
            return;

        Transform spawnPoint = enemyPreviewManager.spawnPoint;
        for (int i = 0; i < spawnPoint.childCount; i++)
            spawnPoint.GetChild(i).gameObject.SetActive(active);
    }

    private void RestoreMainEnemyPreview()
    {
        if (enemyPreviewManager == null)
            return;

        int currentStage = PlayerPrefs.GetInt(StageNoKey, 1);
        enemyPreviewManager.ShowEnemyForStage(currentStage);
    }

    private void BuildLevelPopups()
    {
        levelPopups.Clear();

        for (int i = 1; i <= TotalLevels; i++)
        {
            GameObject instance = Instantiate(popupLVTemplate, contentParent);
            instance.name = $"PopupLV_{i}";
            instance.SetActive(true);

            LevelPopupUI popup = BindPopupUI(instance, i);
            if (popup != null)
                levelPopups.Add(popup);
        }
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent.name == childName)
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindDeepChild(parent.GetChild(i), childName);
            if (result != null)
                return result;
        }

        return null;
    }

    private LevelPopupUI BindPopupUI(GameObject root, int level)
    {
        Transform topFrame = FindDeepChild(root.transform, "TopFrame");
        Transform titleTransform = FindDeepChild(root.transform, "Text_Title");
        Transform buttonOpenTransform = FindDeepChild(root.transform, "Button_Open");
        Transform buttonLockTransform = FindDeepChild(root.transform, "Button_Lock");
        Transform uiLockTransform = FindDeepChild(root.transform, "UILock");
        Transform rawImageTransform = FindDeepChild(root.transform, "RawImage1");

        if (topFrame == null || titleTransform == null || buttonOpenTransform == null ||
            buttonLockTransform == null || uiLockTransform == null || rawImageTransform == null)
        {
            Debug.LogError(
                $"PopupLVManager: Missing UI nodes on {root.name}. " +
                $"TopFrame={(topFrame != null)}, Text_Title={(titleTransform != null)}, " +
                $"Button_Open={(buttonOpenTransform != null)}, Button_Lock={(buttonLockTransform != null)}, " +
                $"UILock={(uiLockTransform != null)}, RawImage1={(rawImageTransform != null)}");
            return null;
        }

        var popup = new LevelPopupUI
        {
            Level = level,
            Root = root,
            TitleText = titleTransform.GetComponent<TextMeshProUGUI>(),
            ButtonOpen = buttonOpenTransform.gameObject,
            ButtonLock = buttonLockTransform.gameObject,
            UILock = uiLockTransform.gameObject,
            AnimalImage = rawImageTransform.GetComponent<RawImage>()
        };

        if (popup.TitleText == null || popup.AnimalImage == null)
        {
            Debug.LogError($"PopupLVManager: Missing TextMeshProUGUI or RawImage on {root.name}.");
            return null;
        }

        popup.AnimalImage.texture = null;
        popup.TitleText.text = $"LEVEL {level}";

        popup.OpenButton = popup.ButtonOpen.GetComponent<Button>();
        if (popup.OpenButton == null)
        {
            popup.OpenButton = popup.ButtonOpen.AddComponent<Button>();
            popup.OpenButton.targetGraphic = popup.ButtonOpen.GetComponent<Image>();
        }

        popup.OpenButton.onClick.RemoveAllListeners();
        int capturedLevel = level;
        popup.OpenButton.onClick.AddListener(() => OnLevelClicked(capturedLevel));

        EnsureNonInteractiveButton(popup.ButtonLock);

        return popup;
    }

    private static void EnsureNonInteractiveButton(GameObject buttonObject)
    {
        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
        {
            button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();
        }

        button.interactable = false;
        button.onClick.RemoveAllListeners();
    }

    public void RefreshAll()
    {
        int unlockedStage = GetUnlockedStage();

        foreach (LevelPopupUI popup in levelPopups)
        {
            if (popup?.OpenButton == null)
                continue;

            ApplyLockState(popup, popup.Level <= unlockedStage);
        }
    }

    private void ApplyLockState(LevelPopupUI popup, bool isUnlocked)
    {
        popup.UILock.SetActive(!isUnlocked);
        popup.ButtonLock.SetActive(!isUnlocked);
        popup.ButtonOpen.SetActive(isUnlocked);
        popup.OpenButton.interactable = isUnlocked;
    }

    private int GetUnlockedStage()
    {
        return Mathf.Max(1, PlayerPrefs.GetInt(StageNoKey, 1));
    }

    private Texture GetAnimalTexture(int levelIndex)
    {
        if (animalTextures != null &&
            levelIndex >= 0 &&
            levelIndex < animalTextures.Length &&
            animalTextures[levelIndex] != null)
        {
            return animalTextures[levelIndex];
        }

        return CaptureAnimalPreview(levelIndex);
    }

    private Texture CaptureAnimalPreview(int levelIndex)
    {
        if (enemyPreviewManager == null ||
            enemyPreviewManager.enemyPrefabs == null ||
            levelIndex < 0 ||
            levelIndex >= enemyPreviewManager.enemyPrefabs.Count ||
            previewCamera == null)
        {
            return null;
        }

        GameObject prefab = enemyPreviewManager.enemyPrefabs[levelIndex];
        if (prefab == null)
            return null;

        ClearTempPreview();

        RenderTexture renderTexture = CreatePreviewRenderTexture();
        renderTexture.Create();
        capturedTextures.Add(renderTexture);

        RenderTexture previousTarget = previewCamera.targetTexture;

        try
        {
            Transform spawnPoint = enemyPreviewManager.spawnPoint;
            tempPreviewObject = Instantiate(
                prefab,
                spawnPoint.position,
                spawnPoint.rotation);

            previewCamera.targetTexture = renderTexture;
            previewCamera.Render();
        }
        finally
        {
            previewCamera.targetTexture = previousTarget;
            ClearTempPreview();
        }

        return renderTexture;
    }

    private void ClearTempPreview()
    {
        if (tempPreviewObject == null)
            return;

        DestroyImmediate(tempPreviewObject);
        tempPreviewObject = null;
    }

    private RenderTexture CreatePreviewRenderTexture()
    {
        if (previewRenderTexture != null)
        {
            return new RenderTexture(
                previewRenderTexture.width,
                previewRenderTexture.height,
                previewRenderTexture.depth,
                previewRenderTexture.format);
        }

        return new RenderTexture(533, 358, 16, RenderTextureFormat.ARGB32);
    }

    private void OnLevelClicked(int level)
    {
        if (level > GetUnlockedStage())
            return;

        if (LifeManager.Instance.CurrentLifes <= 0)
        {
            if (lifePopup != null)
                lifePopup.Init();
            return;
        }

        PlayerPrefs.SetInt(PlayStageNoKey, level);
        PlayerPrefs.Save();
        SceneManager.LoadScene("InGame");
    }

    private class LevelPopupUI
    {
        public int Level;
        public GameObject Root;
        public TextMeshProUGUI TitleText;
        public GameObject UILock;
        public GameObject ButtonOpen;
        public GameObject ButtonLock;
        public RawImage AnimalImage;
        public Button OpenButton;
    }
}
