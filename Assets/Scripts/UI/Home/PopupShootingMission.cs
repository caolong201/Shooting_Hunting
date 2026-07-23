using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupShootingMission : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button closeButton;

    private int pendingLevel;
    private bool initialized;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Initialize()
    {
        if (initialized)
            return;

        initialized = true;

        if (continueButton == null)
        {
            Transform continueTransform = FindDeepChild(transform, "Button_01_l_Green");
            if (continueTransform != null)
                continueButton = EnsureButton(continueTransform.gameObject);
        }

        if (closeButton == null)
        {
            Transform closeTransform = FindDeepChild(transform, "Button_Convex_Circle_02_Red");
            if (closeTransform != null)
                closeButton = EnsureButton(closeTransform.gameObject);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }
    }

    public void Show(int level)
    {
        Initialize();
        pendingLevel = level;

        if (AudioManager.IsInstanceValid())
            AudioManager.Instance.PlayClick();

        gameObject.SetActive(true);
    }

    public void Close()
    {
        if (AudioManager.IsInstanceValid())
            AudioManager.Instance.PlayClick();

        gameObject.SetActive(false);
    }

    public void OnContinueClicked()
    {
        if (pendingLevel <= 0)
            return;

        PlayerPrefs.SetInt(PopupLVManager.PlayStageNoKey, pendingLevel);
        PlayerPrefs.Save();

        if (AudioManager.IsInstanceValid())
            AudioManager.Instance.PlayClick();

        SceneManager.LoadScene("InGame");
    }

    private static Button EnsureButton(GameObject buttonObject)
    {
        Button button = buttonObject.GetComponent<Button>();
        if (button != null)
            return button;

        button = buttonObject.AddComponent<Button>();
        Image image = buttonObject.GetComponent<Image>();
        if (image != null)
            button.targetGraphic = image;

        return button;
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
}
