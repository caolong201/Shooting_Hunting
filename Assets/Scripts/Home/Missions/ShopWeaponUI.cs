using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ShopWeaponUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button BtnSkinWeapon;
    public Button BtnAmmo;
    public Button BtnHat;
    [SerializeField] GameObject UIShopWeapon;
    public ItemSkinWeaponUI[] skinItems;
    [System.Serializable]
    public class TabButton
    {
        public GameObject Content;
        public Image ImagePresent;
        public Image ImageDim;
    }
    public TabButton SkinWeaponTab;
    public TabButton AmmoTab;
    public TabButton HatTab;

   

    private void Start()
    {   
        BtnSkinWeapon.onClick.AddListener(() => ShowTab(0));
        //BtnAmmo.onClick.AddListener(() => ShowTab(1));
        //BtnHat.onClick.AddListener(() => ShowTab(2));
        ShowTab(0);
    }

    private void ShowTab(int index)
    {

        SetTab(SkinWeaponTab, false);
        SetTab(AmmoTab, false);
        SetTab(HatTab, false);
        switch (index)
        {
            case 0:
                SetTab(SkinWeaponTab, true);
                Debug.Log("ContentSkinWeapon");
                break;
            case 1:
                SetTab(AmmoTab, true);
                Debug.Log("ContentAmmo");
                break;
            case 2: 
                SetTab(HatTab, true);
                Debug.Log("ContentHat");
                break;
        }
    }

    private void SetTab(TabButton tab, bool isActive)
    {
        if (tab.Content != null) tab.Content.SetActive(isActive);
        if (tab.ImagePresent != null) tab.ImagePresent.gameObject.SetActive(isActive);
        if (tab.ImageDim != null) tab.ImageDim.gameObject.SetActive(!isActive);
    }


  
    public void BntCloseShowShopWeapon()
    {
        if (AudioManager.IsInstanceValid())
            AudioManager.Instance.PlayClick();

        UIShopWeapon.transform.GetChild(0).DOKill();
        UIShopWeapon.transform.GetChild(0).DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                UIShopWeapon.SetActive(false);
                UIShopWeapon.transform.GetChild(0).localScale = Vector3.one;

            });
    }
}
