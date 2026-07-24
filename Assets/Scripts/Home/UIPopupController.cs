using DG.Tweening;
using System.Collections.Generic;
using System.Security.Claims;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupController : MonoBehaviour
{
    [SerializeField] private GameObject dailyBonusPrefab;
    [SerializeField] private GameObject dailyMissionPrefab;
    [SerializeField] private GameObject ShopWeaponPrefab;
    [SerializeField] private Transform uiDailyBonusParent;
    [SerializeField] private Transform uiSkinWeapon;
    [SerializeField] private GameObject badgeLoginBonus, badgeMission;
    private GameObject dailyBonusInstance;
    private GameObject dailyMissionInstance, ShopWeaponInstance;


    private void Start()
    {
        dailyBonusInstance = Instantiate(dailyBonusPrefab, uiDailyBonusParent);
        dailyBonusInstance.SetActive(false);
        DailyBonus dailyBonus = dailyBonusInstance.GetComponent<DailyBonus>();
        if (dailyBonus != null)
        {
            dailyBonus.OnClaimSuccess = () => { ShowBadgeLoginBonus(false); };

            dailyBonus.OnCloseWithoutClaim = () => { ShowBadgeLoginBonus(true); };
        }

        int claimed = PlayerPrefs.GetInt("DailyBonusClaimed", 0);
        int closedUnclaimed = PlayerPrefs.GetInt("DailyBonusClosedUnclaimed", 0);
        if (claimed == 0 && closedUnclaimed == 0)
        {
            DOVirtual.DelayedCall(1, () => { ShowDailyBonus(); });
        }
        else
        {
            //icClaim.SetActive(false);
            ShowBadgeLoginBonus(claimed == 0 && closedUnclaimed == 1);
        }

        ShowBadgeMission(MissionManager.Instance.HasClaimableMissions());
    }

    public void OnClickUIDaily()
    {
        PlayUiClick();

        if (dailyBonusInstance != null && !dailyBonusInstance.activeSelf)
        {
            ShowBadgeLoginBonus(false);

            ShowDailyBonus();
        }
    }

    public void OnClickUIDailyMission()
    {
        PlayUiClick();

        dailyMissionInstance = Instantiate(dailyMissionPrefab, uiDailyBonusParent);
        dailyMissionInstance.transform.localScale = Vector3.zero;
        dailyMissionInstance.SetActive(true);
        dailyMissionInstance.transform.DOScale(1.1f, 0.2f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                dailyMissionInstance.transform
                    .DOScale(1f, 0.2f)
                    .SetEase(Ease.OutSine);
            });
        DailyMissionUI missionUI = dailyMissionInstance.GetComponent<DailyMissionUI>();
        if (missionUI != null)
        {
            missionUI.LoadMissions();
        }

        ShowBadgeMission(false);
    }

    private void ShowDailyBonus()
    {
        dailyBonusInstance.transform.localScale = Vector3.zero;
        dailyBonusInstance.SetActive(true);
        dailyBonusInstance.transform
            .DOScale(1.1f, 0.2f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                dailyBonusInstance.transform
                    .DOScale(1f, 0.2f)
                    .SetEase(Ease.OutSine);
            });

    }

    public void ShowShopWeapon()
    {
        PlayUiClick();

        ShopWeaponInstance = Instantiate(ShopWeaponPrefab, uiSkinWeapon);
        ShopWeaponInstance.transform.GetChild(0).localScale = Vector3.zero;
        ShopWeaponInstance.SetActive(true);
        ShopWeaponInstance.transform.GetChild(0).DOScale(1.1f, 0.2f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                ShopWeaponInstance.transform.GetChild(0)
                    .DOScale(1f, 0.2f)
                    .SetEase(Ease.OutSine);
            });
        var shopUI = ShopWeaponInstance.GetComponent<ShopWeaponUI>();
        if (shopUI != null)
        {
            WeaponSkinManager.Instance.SetSkinItems(shopUI.skinItems);
            WeaponSkinManager.Instance.RefreshAll();
        }
    }


    public void ShowBadgeLoginBonus(bool show)
    {
        if (badgeLoginBonus != null)
            badgeLoginBonus.SetActive(show);
    }

    public void ShowBadgeMission(bool show)
    {
        if (badgeMission != null)
            badgeMission.SetActive(show);
    }

    private static void PlayUiClick()
    {
        if (AudioManager.IsInstanceValid())
            AudioManager.Instance.PlayClick();
    }
}