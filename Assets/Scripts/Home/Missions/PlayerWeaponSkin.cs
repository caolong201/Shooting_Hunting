
using UnityEngine;

public class PlayerWeaponSkin : SingletonMonoAwake<PlayerWeaponSkin>
{
    [Header("Weapon Material")]
    public Material weaponMaterial;   
    [Header("Skin Textures")]
    public Texture[] skinTextures;                                         
    private int defaultSkinID = 0;
    public Color[] scopeColors;
    public Material[] scopeMaterials;
    private void Start()
    {
        int usedSkinID = SaveDataManager.Instance.LoadUsedSkin();
        ApplySkin(usedSkinID);
    }
    public void ApplySkin(int skinID)
    {
        if (weaponMaterial == null)
        {          
            return;
        }
        if (skinID < 0 || skinID >= skinTextures.Length || skinTextures[skinID] == null)
        {
            weaponMaterial.SetTexture("_MainTex", skinTextures[defaultSkinID]);
            ApplyScopeColor(defaultSkinID);
            return;
        }    
        weaponMaterial.SetTexture("_MainTex", skinTextures[skinID]);       
        ApplyScopeColor(skinID);
    }

    private void ApplyScopeColor(int skinID)
    {
        if (scopeMaterials == null || scopeMaterials.Length == 0) return;
        if (scopeColors == null || scopeColors.Length == 0) return;

        int safeID = Mathf.Clamp(skinID, 0, scopeColors.Length - 1);
        Color targetColor = scopeColors[safeID];

        for (int i = 0; i < scopeMaterials.Length; i++)
        {
            if (scopeMaterials[i] != null)
            {
                scopeMaterials[i].color = targetColor;
            }
        }
    }
    public void ResetToDefault()
    {
        ApplySkin(defaultSkinID);
        SaveDataManager.Instance.SaveUsedSkin(defaultSkinID);
    }
}
