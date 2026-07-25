using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupMessageUI : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public Button okButton;
    public RectTransform popupRect;  
    public void Setup(string message)
    {
        messageText.text = message;
        okButton.onClick.AddListener(() => ClosePopup());
        PlayShowAnimation();
    }

    private void PlayShowAnimation()
    {
        if (popupRect == null)
            popupRect = GetComponent<RectTransform>();

        popupRect.localScale = Vector3.zero; // reset scale
        popupRect.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
    }

    private void ClosePopup()
    {
        if (AudioManager.IsInstanceValid())
            AudioManager.Instance.PlayClick();

        if (popupRect == null)
            popupRect = GetComponent<RectTransform>();

        popupRect.DOScale(Vector3.zero, 0.25f)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
    }
}
