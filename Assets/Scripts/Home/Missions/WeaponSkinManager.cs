using UnityEngine;

public class WeaponSkinManager : SingletonMonoAwake<WeaponSkinManager>
{
    public ItemSkinWeaponUI[] skinItems;   
    private int currentUsedSkinID = -1;    
    [Header("Prefabs")]
    public GameObject popupMessagePrefab;   
    public GameObject popupConfirmPrefab;
    [Header("Parent")]
    public Transform popupParent;  
    public void SetSkinItems(ItemSkinWeaponUI[] items)
    {
        skinItems = items;
    }

    public void Show(string message)
    {
        GameObject popupObj = Instantiate(popupMessagePrefab, popupParent);
        PopupMessageUI popup = popupObj.GetComponent<PopupMessageUI>();
        popup.Setup(message);
    }
    public void ShowConfirm(string message, System.Action callbackConfirm)
    {
        //GameObject popupObj = Instantiate(popupConfirmPrefab, popupParent);
        //PopupConfirmUI popup = popupObj.GetComponent<PopupConfirmUI>();
        //popup.Setup(message, callbackConfirm);
        if (popupConfirmPrefab == null)
        {       
            return;
        }
        if (popupParent == null)
        {          
            return;
        }

        GameObject popupObj = Instantiate(popupConfirmPrefab, popupParent);
        PopupConfirmUI popup = popupObj.GetComponent<PopupConfirmUI>();
        if (popup == null)
        {
            return;
        }

        popup.Setup(message, callbackConfirm);
    }

    public void SetUsedSkin(int id)
    {
        currentUsedSkinID = id;
        Debug.Log($"Skin {id} is in use!");
  
        foreach (var item in skinItems)
        {
            if (item.skinID == id)
            {
               
                item.SetState(SkinState.Used);
            }
            else if (item.GetState() != SkinState.NotBought)
            {
            
                item.SetState(SkinState.Bought);
            }
        }
        SaveDataManager.Instance.SaveUsedSkin(id);
        //PlayerWeaponSkin.Instance.ApplySkin(id);  
    }
    public int GetCurrentSkinID()
    {
        return currentUsedSkinID;
    }
    private void LoadSkins()
    {
       
        int usedSkinID = SaveDataManager.Instance.LoadUsedSkin();

        foreach (var item in skinItems)
        {
            if (item.skinID == 0)
            {
               
                SaveDataManager.Instance.SaveBoughtSkin(0);

                if (usedSkinID == 0 || usedSkinID == -1)
                {
                   
                    item.SetState(SkinState.Used);
                    usedSkinID = 0;
                    SaveDataManager.Instance.SaveUsedSkin(0);
                }
                else
                {
                    
                    item.SetState(SkinState.Bought);
                }
            }
            else
            {
              
                if (SaveDataManager.Instance.IsSkinBought(item.skinID))
                {
                    if (item.skinID == usedSkinID)
                        item.SetState(SkinState.Used);
                    else
                        item.SetState(SkinState.Bought);
                }
                else
                {
                    item.SetState(SkinState.NotBought);
                }
            }
        }

        currentUsedSkinID = usedSkinID;
    }
    public void RefreshAll()
    {
        if (skinItems != null && skinItems.Length > 0)
            LoadSkins();
    }
}
