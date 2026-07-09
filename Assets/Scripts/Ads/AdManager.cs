using System;
using System.Collections;
using System.Collections.Generic;
using CrazyGames;
using UnityEngine;

public class AdManager : SingletonMonoStart<AdManager>
{
    int retryAttempt;

    string videoAdUnitId = "5ca7ecb319c8584d";
    string bannerAdUnitId = "1ad131e368a413bb";
    string intertitialAdUnitId = "330163a9156fa112";

    private Action<bool> onAdRewarded = null;

    public override void OnStart()
    {
        base.OnStart();
        TrackingUtil.Init();

        if (CrazySDK.IsAvailable)
        {
            CrazySDK.Init(() =>
            {
                CrazySDK.Ad.PrefetchAd(CrazyAdType.Rewarded);
            });
            return;
        }

        //Applovin - maxsdk - AD
        AdUtil.Init();

        InitializeBannerAds();
        InitializeRewardedAds();

        // if (PlayerPrefs.GetInt("StageNo", 1) >= 8)
        // {
        //     LoadIntertitialAd();
        // }
    }

    public void InitializeBannerAds()
    {
        MaxSdk.CreateBanner(bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);

        // Set background or background color for banners to be fully functional
        MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, Color.white);

        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
    }

    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        MaxSdk.ShowBanner(bannerAdUnitId);
    }

    private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        InitializeBannerAds();
    }

    public void InitializeRewardedAds()
    {
        // Attach callback
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

        // Load the first rewarded ad
        LoadRewardedAd();
    }


    private void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(videoAdUnitId);
    }

    public bool IsRewardedAdReady()
    {
        return MaxSdk.IsRewardedAdReady(videoAdUnitId);
    }

    public void ShowRewardedAd(System.Action<bool> complete)
    {
        if (CrazySDK.IsAvailable)
        {
            ShowCrazyRewardedAd(complete);
            return;
        }

        this.onAdRewarded = complete;
        if (MaxSdk.IsRewardedAdReady(videoAdUnitId))
        {
            MaxSdk.ShowRewardedAd(videoAdUnitId);
        }
        else
        {
            this.onAdRewarded?.Invoke(false);
        }
    }

    private void ShowCrazyRewardedAd(Action<bool> complete)
    {
        this.onAdRewarded = complete;

        if (!CrazySDK.IsInitialized)
        {
            CrazySDK.Init(() => RequestCrazyRewardedAd());
            return;
        }

        RequestCrazyRewardedAd();
    }

    private void RequestCrazyRewardedAd()
    {
        CrazySDK.Ad.RequestAd(
            CrazyAdType.Rewarded,
            () => { },
            error =>
            {
                Debug.LogWarning("Rewarded ad error: " + error);
                onAdRewarded?.Invoke(false);
                onAdRewarded = null;
            },
            () =>
            {
                onAdRewarded?.Invoke(true);
                MissionManager.Instance.OnRewardedAdWatched();
                onAdRewarded = null;
            }
        );
    }
    
    private void LoadIntertitialAd()
    {
        MaxSdk.LoadInterstitial(intertitialAdUnitId);
    }
    
    public bool IsIntertitialAdReady()
    {
        return MaxSdk.IsInterstitialReady(intertitialAdUnitId);
    }
    
    public void ShowIntertitialAd()
    {
        if (IsIntertitialAdReady())
        {
            MaxSdk.ShowInterstitial(intertitialAdUnitId);
        }
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.

        // Reset retry attempt
        retryAttempt = 0;
    }

    private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Rewarded ad failed to load
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).

        retryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryAttempt));

        Invoke("LoadRewardedAd", (float)retryDelay);
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
        MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
        LoadRewardedAd();
    }


    private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is hidden. Pre-load the next ad
        LoadRewardedAd();
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        print("Rewarded user: " + reward.Amount + " " + reward.Label);
        this.onAdRewarded?.Invoke(true);

        MissionManager.Instance.OnRewardedAdWatched();
    }
}