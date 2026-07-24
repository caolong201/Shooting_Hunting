using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SaveDataManager;

public class HomeController : MonoBehaviour
{
    [SerializeField] LifePopup lifePopup;
    [SerializeField] TextMeshProUGUI txtCurrentLife;
    [SerializeField] private TextMeshProUGUI txtCoin;
    [SerializeField] private TextMeshProUGUI txtGem;


    private void Start()
    {
        lifePopup.gameObject.SetActive(false);
        StartCoroutine(UpdateLifeTimer());
        txtCoin.text = SaveDataManager.Instance.Coin.ToString();
        txtGem.text = SaveDataManager.Instance.Gems.ToString();

        SaveDataManager.Instance.CoinUpdated += OnCoinUpdated;
        SaveDataManager.Instance.GemUpdated += OnGemUpdate;
    }

    //daily
    private void OnDestroy()
    {
        if (SaveDataManager.Instance != null)
        {
            SaveDataManager.Instance.CoinUpdated -= OnCoinUpdated;
            SaveDataManager.Instance.GemUpdated -= OnGemUpdate;

        }
    }

    private void OnCoinUpdated(int newAmount)
    {
        txtCoin.text = newAmount.ToString();
    }
    private void OnGemUpdate(int newGame)
    {
        txtGem.text = newGame.ToString();   
    }

    public void OnclickPlay()
    {
        if (LifeManager.Instance.CurrentLifes <= 0)
        {
            lifePopup.Init();
            return;
        }
    
        SceneManager.LoadScene("InGame");
    }

    public void OnbtnLifeClicked()
    {
        if (AudioManager.IsInstanceValid())
            AudioManager.Instance.PlayClick();

        lifePopup.Init();
    }

    private IEnumerator UpdateLifeTimer()
    {
        while (true)
        {
            txtCurrentLife.text = LifeManager.Instance.CurrentLifes + "/5";
            yield return new WaitForSeconds(60f);
        }
    }
}
