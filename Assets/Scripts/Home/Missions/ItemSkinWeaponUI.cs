using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
public enum SkinState
{
    NotBought,   
    Bought,      
    Used         
}
public class ItemSkinWeaponUI : MonoBehaviour
{
    [Header("UI")]
    public Image skinImage;          
    public Button btnNotBought;     
    public Button btnBought;        
    public Button btnUsed;           
    public TextMeshProUGUI priceText;          

    [Header("Skin Data")]
    public int skinID;               
    public int price;                
    public Sprite icon;              

    private SkinState currentState;  
    private void Start()
    {
        SetupUI();
        btnNotBought.onClick.AddListener(OnClickBuy);
        btnBought.onClick.AddListener(OnClickUse);
        btnUsed.onClick.AddListener(OnClickUse);
    }
    public void Init(int id, int cost, Sprite sprite,    SkinState state)
    {
        skinID = id;
        price = cost;
        icon = sprite;
        currentState = state;
        if (skinImage != null) skinImage.sprite = icon;

        if (skinID == 0)
        {        
            SaveDataManager.Instance.SaveBoughtSkin(0);          
            int usedSkin = SaveDataManager.Instance.LoadUsedSkin();
            currentState = (usedSkin == 0) ? SkinState.Used : SkinState.Bought;

            if (priceText != null) priceText.gameObject.SetActive(false);
        }
        else
        {
            if (priceText != null) priceText.text = price.ToString();
        }
        SetupUI();
    }

    private void SetupUI()
    {
        //btnNotBought.gameObject.SetActive(currentState == SkinState.NotBought);
        //btnBought.gameObject.SetActive(currentState == SkinState.Bought);
        //btnUsed.gameObject.SetActive(currentState == SkinState.Used);
        if (skinID == 0)
        {         
            btnNotBought.gameObject.SetActive(false);
            btnBought.gameObject.SetActive(currentState == SkinState.Bought);
            btnUsed.gameObject.SetActive(currentState == SkinState.Used);
        }
        else
        {
            btnNotBought.gameObject.SetActive(currentState == SkinState.NotBought);
            btnBought.gameObject.SetActive(currentState == SkinState.Bought);
            btnUsed.gameObject.SetActive(currentState == SkinState.Used);
        }
    }

    private void OnClickBuy()
    {
        PlayUiClick();

        if (skinID == 0) return;
        int playerCoins = SaveDataManager.Instance.Coin;

        if (playerCoins < price)
        {
          
            WeaponSkinManager.Instance.Show("Not enough coins to buy!");
            return;
        }
       
        WeaponSkinManager.Instance.ShowConfirm($"Buy skin {skinID} with price {price} coins ?", () =>
        {
            
            SaveDataManager.Instance.UpdateCoins(-price);
            SaveDataManager.Instance.SaveBoughtSkin(skinID);
            WeaponSkinManager.Instance.RefreshAll();
           
            currentState = SkinState.Bought;
            SetupUI();
            MissionManager.Instance.OnAmmoUpgraded();
        });
    }
  
    private void OnClickUse()
    {
        PlayUiClick();

        WeaponSkinManager.Instance.SetUsedSkin(skinID);
        SaveDataManager.Instance.SaveUsedSkin(skinID);
        currentState = SkinState.Used;
        SetupUI();


    }

    private static void PlayUiClick()
    {
        if (AudioManager.IsInstanceValid())
            AudioManager.Instance.PlayClick();
    }
    private void OnClickUnuse()
    {     
        currentState = SkinState.Bought;
        SetupUI();
        SaveDataManager.Instance.SaveUsedSkin(0);
        PlayerWeaponSkin.Instance.ResetToDefault();
    }
    public void SetState(SkinState state)
    {
        currentState = state;
        SetupUI();
    }

    public SkinState GetState()
    {
        return currentState;
    }
}