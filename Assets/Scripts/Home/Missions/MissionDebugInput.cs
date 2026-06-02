using UnityEngine;

public class MissionDebugInput : MonoBehaviour
{
    void Update()
    {
        // LoginDaily
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            MissionManager.Instance.OnDailyLogin();
            Debug.Log(">>> Debug: LoginDaily mission progressed");
        }

        // UpgradeAmmo
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            MissionManager.Instance.OnAmmoUpgraded();
            Debug.Log(">>> Debug: UpgradeAmmo mission progressed");
        }

        // WatchRewardedAds
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            MissionManager.Instance.OnRewardedAdWatched();
            Debug.Log(">>> Debug: WatchRewardedAds mission progressed");
        }

        // CompleteStage
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            MissionManager.Instance.OnStageCompleted(1);
            Debug.Log(">>> Debug: Completed 1 stage");
        }

        // HitBrains
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            MissionManager.Instance.OnBrainHit(5);
            Debug.Log(">>> Debug: Hit 5 brains");
        }

        // HitHearts
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            MissionManager.Instance.OnHeartHit(1);
            Debug.Log(">>> Debug: Hit 1 heart");
        }

        // SpendMoney
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            MissionManager.Instance.OnMoneySpent(10000);
            Debug.Log(">>> Debug: Spent 10k money");
        }

        // SpinRoulette
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            MissionManager.Instance.OnRouletteSpun();
            Debug.Log(">>> Debug: Spun roulette once");
        }

        // CollectCoins
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            MissionManager.Instance.OnCoinsCollected(5000);
            Debug.Log(">>> Debug: Collected 5k coins");
        }

        // OpenTreasureBox
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            MissionManager.Instance.OnTreasureBoxOpened();
            Debug.Log(">>> Debug: Opened treasure box once");
        }

        // KillAnimals
        if (Input.GetKeyDown(KeyCode.E))
        {
            MissionManager.Instance.OnAnimalKilled(5);
            Debug.Log(">>> Debug: Killed 5 animals");
        }

        // KillRunningAnimals
        if (Input.GetKeyDown(KeyCode.R))
        {
            MissionManager.Instance.OnRunningAnimalKilled(3);
            Debug.Log(">>> Debug: Killed 3 running animals");
        }

        // PlayTime (30 phút = 1800s)
        if (Input.GetKeyDown(KeyCode.T))
        {
            MissionManager.Instance.AddPlaySeconds(1800);
            Debug.Log(">>> Debug: Added 30 minutes playtime");
        }
    }
}
