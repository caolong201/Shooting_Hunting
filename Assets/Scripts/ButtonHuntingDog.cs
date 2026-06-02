using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHuntingDog : MonoBehaviour
{
    private Button button;
    [SerializeField] private TextMeshProUGUI tmpText;
    [SerializeField] private Image cooldownFill;
    [SerializeField] private Button bntAds;

    private float cooldown = 300;
    private float cooldownEndTime;

    private Coroutine co;
    private CanvasGroup canvasGroup;

    [SerializeField] private GameManager gameManager;

    void Awake()
    {
        button = GetComponent<Button>();
        canvasGroup = GetComponent<CanvasGroup>();

        SetFill(0f);
        SetText("");
    }

    void OnEnable()
    {
        float remain = cooldownEndTime - Time.unscaledTime;
        if (remain > 0f)
        {
            if (button != null) button.interactable = false;
            co = StartCoroutine(CooldownRoutine(remain));

        }
    }

    public void OnButtonClicked()
    {
        if (button != null) button.interactable = false;

        cooldownEndTime = Time.unscaledTime + cooldown;   
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(CooldownRoutine(cooldown));

    }

    private IEnumerator CooldownRoutine(float duration)
    {
        float endTime = Time.unscaledTime + duration;
        bntAds.gameObject.SetActive(true);
        while (Time.unscaledTime < endTime)
        {
            float remain = endTime - Time.unscaledTime;

            // Update UI
            SetFill(remain / duration);
            SetText(FormatTime(remain));
            yield return null;
        }

        SetFill(0f);
        SetText("");
        if (button != null) button.interactable = true;
        co = null;
        bntAds.gameObject.SetActive(false);
    }


    private void SetFill(float v)
    {
        if (cooldownFill != null)
        {
            v = 1 - v;
            cooldownFill.fillAmount = Mathf.Clamp01(v);
        }
    }

    private void SetText(string s)
    {
        if (tmpText != null) tmpText.text = s;
    }

    private string FormatTime(float seconds)
    {
        return Mathf.CeilToInt(seconds).ToString();
    }

    public void Show(bool show)
    {
        if (show)
        {
            canvasGroup.alpha = 1;
        }
        else
        {
            canvasGroup.alpha = 0;
        }
    }
    public void OnClickbntAds()
    {
        AdManager.Instance.ShowRewardedAd((a) =>
        {
            if (a)
            {
                bntAds.gameObject.SetActive(false);
                CancelCooldown();

            }

        });

    }
    public bool IsShowing()
    {
        return canvasGroup.alpha >= 0.5f;
    }

    public void CancelCooldown()
    {
        if (co != null)
        {
            StopCoroutine(co);
            co = null;
        }

        SetFill(0f);
        SetText("");
        if (button != null) button.interactable = true;
    }
}