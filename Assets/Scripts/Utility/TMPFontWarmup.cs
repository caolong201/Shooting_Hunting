using TMPro;
using UnityEngine;

/// <summary>
/// Preloads TMP glyphs on the main thread to avoid Unity 6 parallel text job races.
/// </summary>
public static class TMPFontWarmup
{
    private static bool _warmedUp;

    private const string CommonCharacters =
        "0123456789:./-%+ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz " +
        "Lives are fullGet more to continue playingFreeREVIVEGive UpFAILED";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Warmup()
    {
        if (_warmedUp)
        {
            return;
        }

        _warmedUp = true;

        if (TMP_Settings.defaultFontAsset != null)
        {
            TMP_Settings.defaultFontAsset.TryAddCharacters(CommonCharacters, out _);
        }

        if (TMP_Settings.fallbackFontAssets != null)
        {
            foreach (var font in TMP_Settings.fallbackFontAssets)
            {
                if (font != null)
                {
                    font.TryAddCharacters(CommonCharacters, out _);
                }
            }
        }
    }
}
