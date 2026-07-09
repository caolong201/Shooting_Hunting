using System;
using System.Collections;
using System.Collections.Generic;
using CrazyGames;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LifePopup : MonoBehaviour
{
    [SerializeField] private Transform root;
    [SerializeField] private TextMeshProUGUI txtCurrentLife;
    [SerializeField] private TextMeshProUGUI txtCurrentLifeTime;
    [SerializeField] private TextMeshProUGUI txtDes;
    
    private int needPayCoin = 1500;

    [SerializeField] Button payCoinButton;
    [SerializeField] TextMeshProUGUI txtMoney;
    
    [SerializeField] private List<GameObject> lstHideWhenLifeFull;
    
    [SerializeField] private TextMeshProUGUI txtCoinInHome;

    private Coroutine _lifeTimerCoroutine;
    private string _lastLifeTimeText;
    private string _lastLifeText;

    private void Awake()
    {
        if (CrazySDK.IsAvailable && !CrazySDK.IsInitialized)
        {
            CrazySDK.Init(() => CrazySDK.Ad.PrefetchAd(CrazyAdType.Rewarded));
        }
    }

    public void Init()
    {
        gameObject.SetActive(true);
        root.localScale = Vector3.one;
        root.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.3f).SetEase(Ease.OutExpo);

        for (int i = 0; i < lstHideWhenLifeFull.Count; i++)
        {
            lstHideWhenLifeFull[i].SetActive(!LifeManager.Instance.IsFullLife());
        }

        if (SaveDataManager.Instance.Coin >= needPayCoin)
        {
            txtMoney.color = Color.green;
            payCoinButton.interactable = true;
        }
        else
        {
            txtMoney.color = Color.red;
            payCoinButton.interactable = false;
        }

        txtCurrentLife.text = LifeManager.Instance.CurrentLifes + "";
        txtCurrentLifeTime.text = LifeManager.Instance.GetTimeUntilNextLife();
        
        if (LifeManager.Instance.IsFullLife())
        {
            StopLifeTimer();
            txtDes.text = "Lives are full"; ;
        }
        else
        {
            txtDes.text = "Get more lives to continue playing";
            StartLifeTimer();
        }

    }

    private void OnDisable()
    {
        StopLifeTimer();
    }

    private void StartLifeTimer()
    {
        StopLifeTimer();
        _lastLifeTimeText = null;
        _lastLifeText = null;
        _lifeTimerCoroutine = StartCoroutine(UpdateLifeTimer());
    }

    private void StopLifeTimer()
    {
        if (_lifeTimerCoroutine != null)
        {
            StopCoroutine(_lifeTimerCoroutine);
            _lifeTimerCoroutine = null;
        }
    }

    public void OnbtnAdClicked()
    {
        if (CrazySDK.IsAvailable)
        {
            ShowCrazyRewardedAd();
            return;
        }

        if (AdManager.IsInstanceValid())
        {
            AdManager.Instance.ShowRewardedAd(OnAdRewardResult);
        }
    }

    private void ShowCrazyRewardedAd()
    {
        Action requestAd = () =>
        {
            CrazySDK.Ad.RequestAd(
                CrazyAdType.Rewarded,
                () => { },
                error => Debug.LogWarning("Rewarded ad error: " + error),
                () =>
                {
                    OnAdRewardResult(true);
                    if (MissionManager.IsInstanceValid())
                    {
                        MissionManager.Instance.OnRewardedAdWatched();
                    }
                }
            );
        };

        if (!CrazySDK.IsInitialized)
        {
            CrazySDK.Init(requestAd);
            return;
        }

        requestAd();
    }

    private void OnAdRewardResult(bool success)
    {
        if (success)
        {
            LifeManager.Instance.AddLife();
            UpdateUI();
        }
    }

    public void OnbtnPayCoinClicked()
    {
        if (SaveDataManager.Instance.Coin >= needPayCoin)
        {
            SaveDataManager.Instance.UpdateCoins(-needPayCoin);
            LifeManager.Instance.AddLife(true);

            MissionManager.Instance.OnMoneySpent(needPayCoin);
            UpdateUI();
        }
    }

     void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            OnbtnPayCoinClicked();

            Debug.LogError("ckeckdelete");
        }
    }
    public void OnbtnCloseClicked()
    {
        StopLifeTimer();
        root.DOScale(new Vector3(0.5f, 0.5f, 1), 0.3f).SetEase(Ease.InExpo)
            .OnComplete(() => { gameObject.SetActive(false); });
    }
    
    private IEnumerator UpdateLifeTimer()
    {
        while (gameObject.activeInHierarchy && !LifeManager.Instance.IsFullLife())
        {
            string lifeTime = LifeManager.Instance.GetTimeUntilNextLife();
            string life = LifeManager.Instance.CurrentLifes + "";

            if (lifeTime != _lastLifeTimeText)
            {
                _lastLifeTimeText = lifeTime;
                txtCurrentLifeTime.SetText(lifeTime);
            }

            if (life != _lastLifeText)
            {
                _lastLifeText = life;
                txtCurrentLife.SetText(life);
            }

            yield return new WaitForSeconds(1f);
        }

        _lifeTimerCoroutine = null;
    }

    private void UpdateUI()
    {
        if (LifeManager.Instance.IsFullLife())
        {
            StopLifeTimer();
            txtDes.text = "Lives are full";
        }
        else
        {
            txtDes.text = "Get more lives to continue playing";
            if (_lifeTimerCoroutine == null && gameObject.activeInHierarchy)
            {
                StartLifeTimer();
            }
        }
        
        for (int i = 0; i < lstHideWhenLifeFull.Count; i++)
        {
            lstHideWhenLifeFull[i].SetActive(!LifeManager.Instance.IsFullLife());
        }

        if (txtCoinInHome != null)
        {
            txtCoinInHome.text = SaveDataManager.Instance.Coin.ToString();
        }
    }
}