using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeManager : MonoBehaviour
{
    [Header("Stage Texts (10 Points)")]
    public List<TextMeshProUGUI> stageTexts; 
    [Header("Progress Slider")]
    public Slider stageSlider;
    private int stagesPerGroup = 10;
    public Image character;

    [Header("Level Play")]
    [SerializeField] private GameObject levelPlayPanel;
    [SerializeField] private GameObject startHuntingButton;

    [Header("Stage Point Backgrounds")]
    public List<Image> stagePointBgs; 
    public Color greenColor = Color.green;
    public Color yellowColor = Color.yellow;
    void Start()
    {
        if (levelPlayPanel != null)
            levelPlayPanel.SetActive(false);

        Button startButton = startHuntingButton != null
            ? startHuntingButton.GetComponent<Button>()
            : null;
        if (startButton != null)
            startButton.onClick.AddListener(OnStartHuntingClicked);

        int currentStage = PlayerPrefs.GetInt("StageNo", 1);
        UpdateStageUI(currentStage);
    }

    public void OnStartHuntingClicked()
    {
        if (levelPlayPanel != null)
            levelPlayPanel.SetActive(true);

        if (startHuntingButton != null)
            startHuntingButton.SetActive(false);

        PopupLVManager popupLvManager = levelPlayPanel != null
            ? levelPlayPanel.GetComponentInChildren<PopupLVManager>(true)
            : FindObjectOfType<PopupLVManager>();

        if (popupLvManager != null)
            popupLvManager.ScrollToLatestUnlocked();
    }
    public void OnWinStage()
    {
        int currentStage = PlayerPrefs.GetInt("StageNo", 1);
        currentStage++;
        PlayerPrefs.SetInt("StageNo", currentStage);
        PlayerPrefs.Save();

        UpdateStageUI(currentStage);

        int indexInGroup = (currentStage - 1) % stagesPerGroup;
        stagePointBgs[indexInGroup].transform
            .DOScale(1.2f, 0.2f)
            .SetLoops(2, LoopType.Yoyo);
    }

    void UpdateStageUI(int currentStage)
    {
        int startStage = ((currentStage - 1) / stagesPerGroup) * stagesPerGroup + 1;


        if (currentStage >= 7)
        {
            character.gameObject.SetActive(false);
        }    


        for (int i = 0; i < stageTexts.Count; i++)
        {
            stageTexts[i].text = (startStage + i).ToString();
        }
        
        
        int indexInGroup = (currentStage - 1) % stagesPerGroup;
      
        for (int i = 0; i < stagePointBgs.Count; i++)
        {
            if (indexInGroup == i)
            {
                stagePointBgs[i].color = greenColor;
            }
            else if ( i < indexInGroup)
            {
                stagePointBgs[i].color = yellowColor;
            }
            else
            {
                stagePointBgs[i].color = Color.gray;
            }
            
           
        }


        if (indexInGroup == 0)
        {
            stageSlider.value = 0f;
        }
        else if (indexInGroup == 1)
        {
            stageSlider.value = 0.2f;
        }
        else
        {

            float extra = (float)(indexInGroup - 2) / (stagesPerGroup - 3);
            stageSlider.value = Mathf.Lerp(0.3f, 1f, extra);
        }
    }
}
