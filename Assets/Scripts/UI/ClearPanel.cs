using DG.Tweening;
using TMPro;
using UnityEngine;

public class ClearPanel : MonoBehaviour
{
    [SerializeField] ArrowSpinner arrowSpinner;
    [SerializeField] private GameObject multply;
    [SerializeField] TextMeshProUGUI textReward, textRewardSpin;
    private bool isStartPin = false;
    private int currReward = 100;
    public void Init(int stage)
    {
        multply.SetActive(true);
        arrowSpinner.StartPin();
        isStartPin = true;
        currReward = 100 + (5 * (stage - 1));
        textReward.text = currReward + "";
        
        GameAnalyticsManager.Instance.TrackEvent($"Stage{stage}:WholeProgress:Level:Complete");
        
        MissionManager.Instance.OnStageCompleted();
    }

    public void OnbtnWatchAdClicked()
    {
        AdManager.Instance.ShowRewardedAd((b) =>
        {
            if (b)
            {
                isStartPin = false;
                int getMultplyCoins = arrowSpinner.StoptPin();
                Debug.Log("MultplyCoins: " + getMultplyCoins);

                AddCoins(currReward, (currReward * getMultplyCoins));
                MissionManager.Instance.OnRouletteSpun();

                MissionManager.Instance.OnTreasureBoxOpened(); /// check xoa

            }
        });
    }

    private void AddCoins(int currentCoins, int amount)
    {
        int startValue = currentCoins;
        int targetValue = currentCoins + amount;

        DOTween.To(() => startValue, x => { textReward.text = x + ""; }, targetValue, 2f)
            .SetEase(Ease.OutQuad).OnComplete(() =>
            {
                multply.SetActive(false);
            });

        textReward.transform.DOScale(1.3f, 0.2f).SetEase(Ease.OutBack)
            .OnComplete(() => textReward.transform.DOScale(1f, 0.2f));
    }

    private void Update()
    {
        if (!isStartPin) return;
        textRewardSpin.text = (arrowSpinner.GetMultiplier() * currReward + currReward) + "";
    }
}