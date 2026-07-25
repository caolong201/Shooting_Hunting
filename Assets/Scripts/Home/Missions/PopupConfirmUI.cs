using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupConfirmUI : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public Button okButton;
    public Button cancelButton;
    public RectTransform popupRect;    
    public void Setup(string message, System.Action onConfirm)
    {
        messageText.text = message;

        okButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            ClosePopup();
        });

        cancelButton.onClick.AddListener(() => ClosePopup());

       
        PlayShowAnimation();
    }

    private void PlayShowAnimation()
    {
        if (popupRect == null)
            popupRect = GetComponent<RectTransform>();
        popupRect.localScale = Vector3.zero;
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