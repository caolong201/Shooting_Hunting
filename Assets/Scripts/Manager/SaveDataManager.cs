using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum EGameState
{
    Live,
    Win,
    Lose
}
public enum EGameMode
{
    BattleBomb,
    BattleGatlingGun,
}

public class SaveDataManager : SingletonMonoAwake<SaveDataManager>
{
    
    public delegate void OnCoinUpdated(int newAmount);
    public event OnCoinUpdated CoinUpdated;


    //gem
    public delegate void OnGemUpdated(int newAmount);
    public event OnGemUpdated GemUpdated;
    private int _gems = 0;
    public int Gems
    {
        set { _gems = value; }
        get { return _gems; }
    }

    private int _coin = 0;

    public int Coin
    {
        set { _coin = value; }
        get { return _coin; }
    }
    
    private int _star = 0;

    public int Star
    {
        set { _star = value; }
        get { return _star; }
    }
    
    private bool isShowTutorial = false;

    public bool IsShowTutorial
    {
        private set { isShowTutorial = value; }
        get { return isShowTutorial; }
    }
 
    public override void OnAwake()
    {
        base.OnAwake();
        isShowTutorial = PlayerPrefs.GetInt("kTutorial", 0) == 0 ? true : false;
        _coin = PlayerPrefs.GetInt("kCoins", 100);

        _gems = PlayerPrefs.GetInt("kGems", 0);
    }


    public void UpdateGems(int amount)
    {
        _gems += amount;
        if (_gems < 0) _gems = 0;
        PlayerPrefs.SetInt("kGems", _gems);
        PlayerPrefs.Save();
        GemUpdated?.Invoke(_gems);
    }


    public void UpdateCoins(int amount)
    {
        _coin += amount;
        PlayerPrefs.SetInt("kCoins", _coin);
        PlayerPrefs.Save();
        CoinUpdated?.Invoke(_coin); 
    }
    
    public void UpdateStar(int amount)
    {
        _star += amount;
        PlayerPrefs.SetInt("kStar", _star);
        PlayerPrefs.Save();
    }

    //Daily
    public void SetClaimedDailyBonus(string today)
    {
        PlayerPrefs.SetString("kDailyBonusClaimed", today);
        PlayerPrefs.Save();
    }

    public bool HasClaimedToday(string today)
    {
        return PlayerPrefs.GetString("kDailyBonusClaimed", "") == today;
    }


    //skin
    public void SaveBoughtSkin(int skinID)
    {
        PlayerPrefs.SetInt($"SkinBought_{skinID}", 1);
        PlayerPrefs.Save();
    }

    public bool IsSkinBought(int skinID)
    {
        return PlayerPrefs.GetInt($"SkinBought_{skinID}", 0) == 1;
    }

    public void SaveUsedSkin(int skinID)
    {
        PlayerPrefs.SetInt("CurrentUsedSkin", skinID);
        PlayerPrefs.Save();
    }

    public int LoadUsedSkin()
    {
        return PlayerPrefs.GetInt("CurrentUsedSkin", -1);
    }

}