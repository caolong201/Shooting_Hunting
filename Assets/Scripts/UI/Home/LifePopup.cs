using System.Collections;
using System.Collections.Generic;
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
            txtDes.text = "Lives are full"; ;
        }
        else
        {
            txtDes.text = "Get more lives to continue playing";
            StartCoroutine(UpdateLifeTimer());
        }

    }

    public void OnbtnAdClicked()
    {
        AdManager.Instance.ShowRewardedAd((b) =>
        {
            if (b)
            {
                LifeManager.Instance.AddLife();
                UpdateUI();
               // GameAnalyticsManager.Instance.TrackEvent($"RewardButton:Clicked:LIVES");
            }
        });
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
        root.DOScale(new Vector3(0.5f, 0.5f, 1), 0.3f).SetEase(Ease.InExpo)
            .OnComplete(() => { gameObject.SetActive(false); });
    }
    
    private IEnumerator UpdateLifeTimer()
    {
        while (true)
        {
            txtCurrentLifeTime.text = LifeManager.Instance.GetTimeUntilNextLife();
            txtCurrentLife.text = LifeManager.Instance.CurrentLifes + "";
            yield return new WaitForSeconds(1f); // Update every second
        }
    }

    private void UpdateUI()
    {
        if (LifeManager.Instance.IsFullLife())
        {
            txtDes.text = "Lives are full";
        }
        else
        {
            txtDes.text = "Get more lives to continue playing";
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