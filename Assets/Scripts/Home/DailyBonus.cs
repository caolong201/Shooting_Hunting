using DG.Tweening;
using System;
using System.Security.Claims;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MaxSdkBase;

public class DailyBonus : MonoBehaviour
{
    [System.Serializable]
    public class RewardFrame
    {
        public GameObject normal;
        public GameObject focus;
        public GameObject dim;
        public GameObject checkIcon;
        public GameObject coinEf;
        public GameObject Gif;
        public GameObject GifOpen;
        //public GameObject CoinDim;

    }
   
    [SerializeField] private RewardFrame[] rewardFrames;
    [SerializeField] GameObject UIDailyBouns;
    [SerializeField] private int[] rewardCoins;
    private int currentDay;
    private DateTime lastClaimTime;
    private const string SaveDayKey = "DailyBonusDay";
    private const string SaveTimeKey = "DailyBonusLastTime";
    private const string SaveClaimedKey = "DailyBonusClaimed";
    private const string SaveClosedUnclaimedKey = "DailyBonusClosedUnclaimed";
    private bool shouldAutoShow;

    public System.Action OnClaimSuccess;
    public System.Action OnCloseWithoutClaim;
    private void Start()
    {
        currentDay = PlayerPrefs.GetInt(SaveDayKey, 0);
        string savedTime = PlayerPrefs.GetString(SaveTimeKey, "");
        int claimed = PlayerPrefs.GetInt(SaveClaimedKey, 0);
        if (!string.IsNullOrEmpty(savedTime))
            lastClaimTime = DateTime.Parse(savedTime);
        else
            lastClaimTime = DateTime.MinValue;

        if (lastClaimTime != DateTime.MinValue)
        {
            TimeSpan timePassed = DateTime.Now - lastClaimTime;

            if (timePassed.TotalHours >= 48)
            {
                ResetToDayOne();
                shouldAutoShow = true;
            }
            else if (claimed == 1 && lastClaimTime.Date < DateTime.Now.Date)
            {
                NextDay();
                shouldAutoShow = true;
                ResetToDayOne();

            }
            else if (claimed == 0 && lastClaimTime.Date < DateTime.Now.Date)
            {
                shouldAutoShow = true;
            }
        }
        else
        {
            shouldAutoShow = true;
        }
        SetupFrames();
    }
    //check
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            ResetTestDays();
        }
        if (Input.GetKeyDown(KeyCode.P)) { Simulate48hPassed(); }

    }
    public void ResetTestDays()
    {
        NextDay();
        SetupFrames();
    }

    private void NextDay()
    {
        currentDay++;
        if (currentDay >= rewardFrames.Length)
            currentDay = 0;

        PlayerPrefs.SetInt(SaveDayKey, currentDay);
        PlayerPrefs.SetInt(SaveClaimedKey, 0); 
        PlayerPrefs.SetInt(SaveClosedUnclaimedKey, 0);
        PlayerPrefs.Save();
    }
    public void Simulate48hPassed()
    {
        lastClaimTime = DateTime.Now.AddHours(-49);
        PlayerPrefs.SetString(SaveTimeKey, lastClaimTime.ToString());
        ResetToDayOne();
        SetupFrames();
    }
    private void ResetToDayOne()
    {
        currentDay = 0;
        PlayerPrefs.SetInt(SaveDayKey, currentDay);
        PlayerPrefs.SetInt(SaveClaimedKey, 0);
        PlayerPrefs.SetInt(SaveClosedUnclaimedKey, 0);
        PlayerPrefs.Save();
    }

    private void SetupFrames()
    {
        for (int i = 0; i < rewardFrames.Length; i++)
        {
            rewardFrames[i].normal.SetActive(true);
            rewardFrames[i].focus.SetActive(false);
            rewardFrames[i].dim.SetActive(false);


            if (rewardFrames[i].coinEf != null)
                rewardFrames[i].coinEf.SetActive(false);

            if (rewardFrames[i].Gif != null)
                rewardFrames[i].Gif.SetActive(false);


            if (rewardFrames[i].GifOpen != null)
                rewardFrames[i].GifOpen.SetActive(false);

        }
        int claimed = PlayerPrefs.GetInt(SaveClaimedKey, 0);


        for (int i = 0; i < currentDay; i++)
        {
            rewardFrames[i].normal.SetActive(false);
            rewardFrames[i].focus.SetActive(false);
            rewardFrames[i].dim.SetActive(true);

            //dticon
            if (rewardFrames[i].checkIcon != null)
                rewardFrames[i].checkIcon.SetActive(true);
        }

        if (claimed == 1)
        {
            rewardFrames[currentDay].dim.SetActive(true);
            ////dticon
            if (rewardFrames[currentDay].checkIcon != null)
                rewardFrames[currentDay].checkIcon.SetActive(true);
        }
        else
        {
            rewardFrames[currentDay].focus.SetActive(true);

            if (rewardFrames[currentDay].Gif != null)
            {
                var gif = rewardFrames[currentDay].Gif;
                gif.SetActive(true);
                gif.transform.DOKill(); 
                gif.transform.localScale = Vector3.one;

                Sequence seq = DOTween.Sequence();
                seq.Append(gif.transform.DOShakePosition(1f, strength: new Vector3(5f, 5f, 0f), vibrato: 10, randomness: 90, snapping: false, fadeOut: true));
                seq.Join(gif.transform.DOScale(1.1f, 0.5f).SetLoops(2, LoopType.Yoyo));
                seq.SetLoops(-1, LoopType.Restart); 
            }
        }

    }

    public void ClaimReward(int dayIndex)
    {
        int claimed = PlayerPrefs.GetInt(SaveClaimedKey, 0);

        if (dayIndex == currentDay && claimed == 0)
        {
            int reward = 0;
            if (rewardCoins != null && dayIndex < rewardCoins.Length)
            {
                /*int*/ reward = rewardCoins[dayIndex];
                SaveDataManager.Instance.UpdateCoins(reward);
                MissionManager.Instance.OnCoinsCollected(reward);
            }
            // Set UI
            rewardFrames[dayIndex].normal.SetActive(false);
            rewardFrames[dayIndex].focus.SetActive(false);
            rewardFrames[dayIndex].dim.SetActive(true);

          
            var gifOpen = rewardFrames[dayIndex].GifOpen;
            var icon = rewardFrames[dayIndex].checkIcon;
            var coinEf = rewardFrames[dayIndex].coinEf;
            Sequence seq = DOTween.Sequence();

            if (gifOpen != null)
            {
                gifOpen.SetActive(true);
                var cg = gifOpen.GetComponent<CanvasGroup>();
                if (cg == null) cg = gifOpen.AddComponent<CanvasGroup>();
                cg.alpha = 1.2f;

                gifOpen.transform.localScale = Vector3.zero;
                seq.Append(gifOpen.transform.DOScale(1.9f, 1f).SetEase(Ease.OutBack));
                seq.Join(cg.DOFade(0f, 0.9f).SetDelay(0.02f));
            }

            if (coinEf != null)
            {
                seq.AppendCallback(() =>
                {
                    PlayCoinPopup(coinEf, reward);
                });
                seq.AppendCallback(() => gifOpen.SetActive(false));
            }

        
            if (icon != null)
            {
                seq.AppendInterval(1.1f); 
                seq.AppendCallback(() =>
                {
                    icon.SetActive(true);
                    icon.transform.localScale = Vector3.zero;
                    Sequence tickSeq = DOTween.Sequence();
                    tickSeq.Append(icon.transform.DOScale(1.2f, 0.4f).SetEase(Ease.OutBack));
                    tickSeq.Append(icon.transform.DOScale(1f, 0.2f).SetEase(Ease.InBack));
                });
            }


            lastClaimTime = DateTime.Now;
            PlayerPrefs.SetString(SaveTimeKey, lastClaimTime.ToString());
            PlayerPrefs.SetInt(SaveDayKey, currentDay);
            PlayerPrefs.SetInt(SaveClaimedKey, 1);

            PlayerPrefs.SetInt(SaveClosedUnclaimedKey, 0);
            PlayerPrefs.Save();
            var uiController = FindObjectOfType<UIPopupController>();
            if (uiController != null)
                uiController.ShowBadgeLoginBonus(false);

            shouldAutoShow = false;

            OnClaimSuccess?.Invoke();
        }
       
    }
    private void PlayCoinPopup(GameObject coinEf, int reward)
    {
        coinEf.SetActive(true);

        var text = coinEf.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null) text.text = $"+{reward}";

        var rect = coinEf.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;

        var cg = coinEf.GetComponent<CanvasGroup>();
        if (cg == null) cg = coinEf.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOAnchorPosY(100f, 0.2f).SetRelative().SetEase(Ease.OutCubic)); 
        seq.AppendInterval(0.2f); 
        seq.Append(rect.DOAnchorPosY(60f, 0.2f).SetRelative().SetEase(Ease.InCubic)); 
        seq.Join(cg.DOFade(0f, 0.7f)); 

        seq.OnComplete(() =>
        {
            coinEf.SetActive(false);
            rect.anchoredPosition = Vector2.zero;
            cg.alpha = 1f;
        });
    }


    public void BntClosUIDaily()    
    {
        UIDailyBouns.transform.DOKill();
        UIDailyBouns.transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                UIDailyBouns.SetActive(false);
                UIDailyBouns.transform.localScale = Vector3.one;
                int claimed = PlayerPrefs.GetInt(SaveClaimedKey, 0);
                if (claimed == 0)
                {
                    PlayerPrefs.SetInt(SaveClosedUnclaimedKey, 1);
                    PlayerPrefs.Save();

                    var uiController = FindObjectOfType<UIPopupController>();
                    if (uiController != null)
                        uiController.ShowBadgeLoginBonus(true);
                }
            });
    }
}

