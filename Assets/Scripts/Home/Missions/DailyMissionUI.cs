using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class ChestPoint
{
    public int point;
    public GameObject chestClosed;
    public GameObject chestOpened;
    public GameObject chestPoint;
    [HideInInspector] public bool opened = false;
}

public class DailyMissionUI : MonoBehaviour
{
    [SerializeField] GameObject UIDailyMission;
    [SerializeField] private Transform missionContentParent;
    [SerializeField] private GameObject missionItemPrefab;
    [Header("Big Progress")]
    [SerializeField]
    private Slider bigSlider;
    private int currentStars = 0;
    private int currentBatchIndex = 0;
    private const int missionsPerBatch = 5;
    private const int starsPerBatch = 100;
    private List<MissionState> allMissions;

    [Header("Chest Points")]
    [SerializeField] private List<ChestPoint> chests = new List<ChestPoint>();

    public GameObject GifPoin;
    public GameObject CoinGif;

    public void LoadMissions()
    {
        allMissions = MissionManager.Instance.GetMissions();
        currentBatchIndex = PlayerPrefs.GetInt("DailyMissionBatch", 0);
        currentStars = PlayerPrefs.GetInt("DailyMissionStars", 0);
        MissionManager.Instance.SetCurrentBatchIndex(currentBatchIndex);
        LoadMissionsBatch(currentBatchIndex);

        CheckChestProgress(currentStars, false);
    }
    private void LoadMissionsBatch(int batchIndex)
    {
        foreach (Transform child in missionContentParent)
            Destroy(child.gameObject);

        int start = batchIndex * missionsPerBatch;
        int end = Mathf.Min(start + missionsPerBatch, allMissions.Count);

        for (int i = start; i < end; i++)
        {
            var mission = allMissions[i];
            GameObject obj = Instantiate(missionItemPrefab, missionContentParent);
            obj.transform.localScale = Vector3.one * 0.9f;
            MissionItemUI ui = obj.GetComponent<MissionItemUI>();
            ui.Setup(mission, this);
        }
        UpdateBigSlider();
    }

    public void OnMissionClaimed(int stars)
    {
        int oldStars = currentStars;
        currentStars += stars;
        if (currentStars > starsPerBatch) currentStars = starsPerBatch;
        PlayerPrefs.SetInt("DailyMissionStars", currentStars);
        PlayerPrefs.Save();
        float oldValue = (float)oldStars / starsPerBatch;
        float newValue = (float)currentStars / starsPerBatch;


        bigSlider.DOKill();
        bigSlider.value = oldValue;

        bigSlider.DOValue(newValue, 0.5f).SetEase(Ease.OutQuad)
            .OnUpdate(() =>
            {
                float sliderStars = bigSlider.value * starsPerBatch;
                CheckChestProgress(Mathf.RoundToInt(sliderStars));
            })
            .OnComplete(() =>
            {
                if (currentStars >= starsPerBatch)
                {
                    ClaimBatchReward();
                }
            });
    }
    private void CheckChestProgress(int starValue, bool playEffect = true)
    {
        foreach (var chest in chests)
        {
            if (starValue >= chest.point)
            {
                if (!chest.opened)
                {
                    chest.opened = true;
                    if (playEffect)
                    {
                        PlayChestOpenEffect(chest);
                    }
                    else
                    {

                        if (chest.chestClosed != null) chest.chestClosed.SetActive(false);
                        if (chest.chestOpened != null) chest.chestOpened.SetActive(true);
                    }
                }
            }
        }
    }
    private Sequence PlayChestOpenEffect(ChestPoint chest)
    {
        if (chest.chestClosed == null || chest.chestOpened == null) return null;

        // Reset trạng thái ban đầu
        chest.chestClosed.SetActive(true);
        chest.chestOpened.SetActive(false);

        Image closedImg = chest.chestClosed.GetComponent<Image>();
        if (closedImg != null)
            closedImg.color = Color.white;

        chest.chestClosed.transform.localScale = Vector3.one;

        Sequence seq = DOTween.Sequence();


        seq.Append(chest.chestClosed.transform.DOScale(1.3f, 0.25f).SetEase(Ease.OutBack));
        if (closedImg != null)
            seq.Join(closedImg.DOFade(0f, 0.25f));

        seq.AppendCallback(() =>
        {
            chest.chestClosed.SetActive(false);
            chest.chestOpened.SetActive(true);

            chest.chestOpened.transform.localScale = Vector3.one * 1.5f;
            var openImg = chest.chestOpened.GetComponent<Image>();
            if (openImg != null)
                openImg.color = new Color(1, 1, 1, 0);
        });


        var openedImg = chest.chestOpened.GetComponent<Image>();
        seq.Append(chest.chestOpened.transform.DOScale(1.1f, 0.25f).SetEase(Ease.OutBack));
        if (openedImg != null)
            seq.Join(openedImg.DOFade(1f, 0.25f));


        seq.Append(chest.chestOpened.transform.DOScale(1f, 0.15f).SetEase(Ease.OutSine));


        if (chest.chestPoint != null)
        {
            chest.chestPoint.SetActive(true);

            CanvasGroup cg = chest.chestPoint.GetComponent<CanvasGroup>();
            if (cg == null) cg = chest.chestPoint.AddComponent<CanvasGroup>();
            cg.alpha = 0;

            RectTransform rt = chest.chestPoint.GetComponent<RectTransform>();
            Vector3 startPos = rt.anchoredPosition;
            Vector3 targetPos = startPos + new Vector3(0, 120f, 0);

            seq.AppendCallback(() =>
            {
                rt.anchoredPosition = startPos;
                cg.alpha = 0;
            });

            seq.Append(rt.DOAnchorPos(targetPos, 0.6f).SetEase(Ease.OutCubic));
            seq.Join(cg.DOFade(1f, 0.3f));
            seq.Append(cg.DOFade(0f, 0.3f).SetDelay(0.1f));
        }
        return seq;
    }
    private void UpdateBigSlider()
    {
        float percent = (float)currentStars / starsPerBatch;
        bigSlider.value = percent;
    }

    private void ClaimBatchReward()
    {

        SaveDataManager.Instance.UpdateCoins(1000);
        SaveDataManager.Instance.UpdateGems(5);
        currentStars = 0;
        PlayerPrefs.SetInt("DailyMissionStars", currentStars);
        currentBatchIndex++;
        PlayerPrefs.SetInt("DailyMissionBatch", currentBatchIndex);
        PlayerPrefs.Save();
        MissionManager.Instance.SetCurrentBatchIndex(currentBatchIndex);

        foreach (var chest in chests)
        {
            chest.opened = false;
        }

        ChestPoint lastChest = chests[chests.Count - 1];
        Sequence seq = PlayChestOpenEffect(lastChest);

        if (seq != null)
        {
            seq.OnComplete(() =>
            {
                Sequence afterChest = PlayBatchCompleteEffect();

                afterChest.OnComplete(() =>
                {
                    foreach (var chest in chests)
                    {
                        if (chest.chestClosed != null)
                        {
                            chest.chestClosed.SetActive(true);
                            var img = chest.chestClosed.GetComponent<Image>();
                            if (img != null) img.color = Color.white;
                            chest.chestClosed.transform.localScale = Vector3.one;
                        }
                        if (chest.chestOpened != null)
                        {
                            chest.chestOpened.SetActive(false);
                        }
                        if (chest.chestPoint != null)
                        {
                            chest.chestPoint.SetActive(false);
                        }
                    }

                    if (GifPoin != null)
                    {
                        GifPoin.SetActive(true);
                        GifPoin.transform.localScale = Vector3.one;
                        var cg = GifPoin.GetComponent<CanvasGroup>();
                        if (cg != null) cg.alpha = 1;
                    }
                    if (CoinGif != null)
                    {
                        CoinGif.SetActive(false);
                    }


                    if (currentBatchIndex * missionsPerBatch < allMissions.Count)
                    {
                        LoadMissionsBatch(currentBatchIndex);
                    }
                    else
                    {
                        Debug.Log("All missions completed → wait for tomorrow reset!");
                    }
                });
            });
        }

        else
        {

            if (currentBatchIndex * missionsPerBatch < allMissions.Count)
            {
                ResetGifAndCoin();
                LoadMissionsBatch(currentBatchIndex);
            }
        }
    }
    private void ResetGifAndCoin()
    {
        if (GifPoin != null)
        {
            GifPoin.SetActive(true);
            GifPoin.transform.localScale = Vector3.one;
            var cg = GifPoin.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1;
        }
        if (CoinGif != null)
        {
            CoinGif.SetActive(false);
            CoinGif.transform.localScale = Vector3.one;
        }
    }

    private Sequence PlayBatchCompleteEffect()
    {
        if (GifPoin == null || CoinGif == null) return null;       
        GifPoin.SetActive(true);
        GifPoin.transform.localScale = Vector3.zero;
        var cg = GifPoin.GetComponent<CanvasGroup>();
        if (cg == null) cg = GifPoin.AddComponent<CanvasGroup>();
        cg.alpha = 1;
        Sequence seq = DOTween.Sequence();

        seq.Append(GifPoin.transform.DOScale(1.2f, 0.4f).SetEase(Ease.OutBack));
        seq.Join(cg.DOFade(0f, 0.4f).SetEase(Ease.InSine));

        seq.AppendCallback(() =>
        {
            GifPoin.SetActive(false);       
            CoinGif.SetActive(true);
            CoinGif.transform.localScale = Vector3.zero;
        });

        seq.Append(CoinGif.transform.DOScale(1.3f, 0.5f).SetEase(Ease.OutBack));
        return seq;
    }

    public bool IsMissionInCurrentBatch(MissionState state)
    {
        int start = currentBatchIndex * missionsPerBatch;
        int end = Mathf.Min(start + missionsPerBatch, allMissions.Count);
        int index = allMissions.IndexOf(state);
        return index >= start && index < end;
    }

    public void BntClosUIDailyMission()
    {
        if (AudioManager.IsInstanceValid())
            AudioManager.Instance.PlayClick();

        UIDailyMission.transform.DOKill();
        UIDailyMission.transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                UIDailyMission.SetActive(false);
                UIDailyMission.transform.localScale = Vector3.one;
            });
    }

}