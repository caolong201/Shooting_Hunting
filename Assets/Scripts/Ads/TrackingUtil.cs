using UnityEngine;
using AdjustSdk;

public static class TrackingUtil
{
    private static readonly string ADJUST_AP_TOKEN = "a7y0grjw5xxc";

    private static bool _isInitialized = false;

    public static void Init()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;

        // adjust 
        InitializeAdjust();
    }

    #region Adjust

    private static void InitializeAdjust()
    {
        AdjustConfig adjustConfig = new AdjustConfig(
            ADJUST_AP_TOKEN,
            AdjustEnvironment.Production,
            true
        );

        adjustConfig.LogLevel = AdjustLogLevel.Info;
        adjustConfig.IsSendingInBackgroundEnabled = true;

        new GameObject("Adjust").AddComponent<Adjust>();

        Adjust.InitSdk(adjustConfig);
    }

    #endregion
}