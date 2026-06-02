using DG.Tweening;
using TMPro;
using UnityEngine;

public class WaveHintAnimation : MonoBehaviour
{
    public float moveDuration = 0.5f;
    public float waitDuration = 1f;
    public float offscreenOffset = 800f;
    
    private RectTransform textRect;
    private TextMeshProUGUI txtText;

    public void ShowWaveHint(string text)
    {
        textRect = GetComponent<RectTransform>();
        txtText = GetComponent<TextMeshProUGUI>();
        txtText.text = text;
        Vector2 startPos = new Vector2(-offscreenOffset, textRect.anchoredPosition.y);
        Vector2 centerPos = new Vector2(0, textRect.anchoredPosition.y);
        Vector2 endPos = new Vector2(offscreenOffset, textRect.anchoredPosition.y);

        textRect.anchoredPosition = startPos;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(textRect.DOAnchorPos(centerPos, moveDuration))
            .AppendInterval(waitDuration)
            .Append(textRect.DOAnchorPos(endPos, moveDuration));
    }
}
