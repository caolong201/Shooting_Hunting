using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]

public class MissionItemUI : MonoBehaviour
{
    public TextMeshProUGUI descriptionText, txtProgress;

    public Slider progressSlider;

    //public Button claimButton;
    public TextMeshProUGUI rewardText;
    private MissionState missionState;
    private int currentProgress = 0;

    [Header("Buttons")] 
    public GameObject normalButton;
    public Button claimButton;
    public GameObject dimButton;
    public GameObject CoinEf; 
    public GameObject checkIcon;    
    private DailyMissionUI parentUI;
    public void Setup(MissionState data, DailyMissionUI parent)
    {
        missionState = data;
        parentUI = parent;

        Refresh(missionState);
        MissionManager.Instance.OnMissionProgress += OnMissionProgress;
        MissionManager.Instance.OnMissionCompleted += OnMissionProgress;
        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(OnClaimReward);
        //checkIcon.transform.localScale = Vector3.zero; // reset ban đầu
        CoinEf.SetActive(false);
      
    }

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionProgress -= OnMissionProgress;
            MissionManager.Instance.OnMissionCompleted -= OnMissionProgress;
        }
    }
    private void OnMissionProgress(MissionState state)
    {
        
        if (state.Def.ID == missionState.Def.ID && parentUI.IsMissionInCurrentBatch(state))
        {
            Refresh(state);
        }
    }
    public void Refresh(MissionState state)
    {
        missionState = state;

        descriptionText.text = $"{missionState.Def.LIST}";
        //rewardText.text = missionState.Def.Reward.ToString();

        txtProgress.text = $"{FormatNumber(missionState.Prog.Current)}/{FormatNumber(missionState.Def.COUNT)}";
        progressSlider.minValue = 0;
        progressSlider.maxValue = missionState.Def.COUNT;
        progressSlider.value = missionState.Prog.Current;

        UpdateButtonState();
    }

    private string FormatNumber(int number)
    {
        if (number >= 1000000)
            return (number / 1000000f).ToString("0.#") + "M";
        if (number >= 1000)
            return (number / 1000f).ToString("0.#") + "K";
        return number.ToString();
    }

    private void UpdateButtonState()
    {
        normalButton.SetActive(false);
        claimButton.gameObject.SetActive(false);
        dimButton.SetActive(false);

        if (missionState.Prog.Claimed)
        {
            dimButton.SetActive(true);
        }
        else if (missionState.Prog.Completed)
        {
            claimButton.gameObject.SetActive(true);
        }
        else
        {
            normalButton.SetActive(true);
        }
    }

    private void OnClaimReward()
    {
        if (missionState.Prog.Claimed) return;

        if (MissionManager.Instance.TryClaim(missionState.Def.ID, out var reward, out var stars))
        {
            Debug.Log($"Claim mission {missionState.Def.ID}, reward {reward}, stars {stars}");
            //SaveDataManager.Instance.UpdateCoins(reward);
            if (parentUI != null)
                parentUI.OnMissionClaimed(stars);      
            missionState.Prog.Claimed = true;
            Refresh(missionState);
            PlayClaimAnimation();
        }
        
    }
    private void PlayClaimAnimation()
    {
        CoinEf.SetActive(true);
        var coinImg = CoinEf.GetComponent<Image>();
        coinImg.color = new Color(1, 1, 1, 1); 
        CoinEf.transform.localPosition = Vector3.zero;
        CoinEf.transform.DOLocalMoveY(140f, 1f).SetEase(Ease.OutQuad);
        coinImg.DOFade(0f, 1f).OnComplete(() => CoinEf.SetActive(false));

     
        checkIcon.SetActive(true);
        checkIcon.transform.localScale = Vector3.zero;
        checkIcon.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            checkIcon.transform.DOScale(1f, 0.1f);
        });
    }
}