using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using IE.RSB;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;


public class UIManager : MonoBehaviour
{
    //時間を表示するText型の変数
    public TextMeshProUGUI timeText;

    public GameObject PopupTimeUp;

    [SerializeField] private GameObject LoadingScene = null;

    [SerializeField] private GameObject InGameUI = null;
    [SerializeField] private CanvasGroup HUD = null;
    [SerializeField] private GameObject FaildUI = null;
    [SerializeField] private ClearPanel ClearUI = null;

    [SerializeField] private RectTransform ScopeUnmask = null;
    [SerializeField] private TextMeshProUGUI bulletText = null;
    [SerializeField] private PlayerWeaponController m_weaponController = null;

    private float tallScreenY = 80f; // 縦長画面の時のY座標
    private float wideScreenY = 40f; // 横長画面の時のY座標
    private float aspectThreshold = 1.8f;

    [SerializeField] public List<GameObject> TargetUI;

    //Getcomponent<Image>.color = new Color32 (242, 108, 216, 255);
    [SerializeField] private List<GameObject> ClearStar = new List<GameObject>();
    [SerializeField] private List<GameObject> ClearCheck = new List<GameObject>();
    [SerializeField] private List<TextMeshProUGUI> ClearCheckText = new List<TextMeshProUGUI>();


    [SerializeField] private Color crosshairNormalColor = Color.white;
    [SerializeField] private Color crosshairEnemyColor = Color.red;
    [SerializeField] private float enemyDetectionRange = 100f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Image crosshair;


    [Header("Hit Indicator")] [SerializeField]
    private GameObject HitImg;

    [SerializeField] private GameObject HeadshotImg;

    [SerializeField] private float hitShowTime = 1f;
    private Coroutine hitCoroutine;

    [SerializeField] TouchCameraRotation touchCameraRotation;
    private Ray ray;
    private float timerZoom = 1;
    private bool isDeepZoom = false;
    private Enemy enemyHit = null;

    [SerializeField] private WaveHintAnimation waveHint;

    [SerializeField] private GameObject PauseUI;
    [SerializeField] private Transform uiParent;
    [SerializeField] private GameObject QuitpauseUI;
    private GameObject currentPauseUI;
    private GameObject currentQuitPauseUI;
    [SerializeField] private EnemyIndicator enemyIndicator;
 
    void Awake()
    {
        for (int i = 0; i < ClearStar.Count; i++)
        {
            ClearStar[i].SetActive(false);
            ClearCheck[i].GetComponent<Image>().color = new Color32(255, 255, 255, 0);
            ClearCheckText[i].text = "";
        }


        if (HitImg) HitImg.SetActive(false);
        if (HeadshotImg) HeadshotImg.SetActive(false);

   
    }

    // Start is called before the first frame update
    void Start()
    {
        crosshair = crosshair.GetComponent<Image>();
        crosshair.color = crosshairNormalColor;

        AdjustPosition();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCrosshairHighlight();
    }

    private void UpdateCrosshairHighlight()
    {
        if (firePoint == null || crosshair == null) return;
        if(!m_weaponController.isAiming) return;

        ray = new Ray(firePoint.position, firePoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, enemyDetectionRange, enemyLayer))
        {
            enemyHit = hit.transform.GetComponentInParent<Enemy>();
            enemyHit.isRayHit = true;
            crosshair.color = crosshairEnemyColor;
            if (!isDeepZoom)
            {
                timerZoom -= Time.deltaTime;
                if (timerZoom <= 0)
                {
                    touchCameraRotation.UpdateZoom(true);
                    timerZoom = 1;
                    isDeepZoom = true;
                }
            }
        }
        else
        {
            if (enemyHit != null)
            {
                enemyHit.isRayHit = false;
                enemyHit = null;
            }
           
            crosshair.color = crosshairNormalColor;
            if (isDeepZoom)
            {
                timerZoom -= Time.deltaTime;
                if (timerZoom <= 0)
                {
                    touchCameraRotation.UpdateZoom(false);
                    timerZoom = 1;
                    isDeepZoom = false;
                }
            }
        }
    }

    public void TimeCountDown(float time)
    {
        if (time > 5)
        {
            timeText.text = time.ToString("f1");
        }
        else
        {
            timeText.text = "<color=red>" + time.ToString("f1") + "</color>";
        }

        //0の場合TimeUp
        if (time < 0.8f)
        {
            if (!PopupTimeUp.activeSelf) PopupTimeUp.SetActive(true);
        }
    }

    public void LoadingComplete()
    {
        //LoadingScene.SetActive(false);
        InGameUI.SetActive(true);
    }

    public void LoadingStart()
    {
        InGameUI.SetActive(false);
        //LoadingScene.SetActive(true);
    }

    void AdjustPosition()
    {
        if (ScopeUnmask == null) return;

        // 現在のアスペクト比（画面の幅 ÷ 高さ）
        float aspectRatio = Screen.height / (float)Screen.width;

        // 縦長か横長か判定
        float newY = (aspectRatio > aspectThreshold) ? tallScreenY : wideScreenY;

        // y軸の変更（x, y, z のうち y だけ変更）
        SetStretchedRectOffset(ScopeUnmask, 55, newY, 55, newY);
    }

    public static void SetStretchedRectOffset(RectTransform rectT, float left, float top, float right, float bottom)
    {
        rectT.offsetMin = new Vector2(left, bottom);
        rectT.offsetMax = new Vector2(-right, -top);
    }

    public void SetBulletCount(int bulletCount)
    {
        m_weaponController.m_ammoInMagazine = bulletCount;
        m_weaponController.m_availableAmmoNow = bulletCount;
        bulletText.text = bulletCount.ToString("000");
    }

    public void SetStageName(string name)
    {
        timeText.text = name;
    }

    public void FaildUIChange()
    {
        FaildUI.SetActive(!FaildUI.activeSelf);
        HUDChange();
    }

    public void ClearUIChange()
    {
        ClearUI.gameObject.SetActive(!ClearUI.gameObject.activeSelf);
        if (ClearUI.gameObject.activeSelf)
        {
            ClearUI.Init(PlayerPrefs.GetInt("StageNo", 1));
        }

        HUDChange();
    }

    public void ClearUIClose()
    {
        ClearUI.gameObject.SetActive(false);
        HUDChange();
    }

    public void ClearStatusUIChange(int no)
    {
        if (no > 3) return;
        ClearStar[no].SetActive(true);
        ClearCheck[no].GetComponent<Image>().color = new Color32(0, 255, 0, 255);
    }

    public void ClearText(int no, int targetNo)
    {
        ClearCheckText[no].text = targetNo.ToString() + " or more bullets remaining";
    }

    public void HUDChange()
    {
        HUD.alpha = 0;
    }

    public void HUDView()
    {
        HUD.alpha = 1;
    }

    public void TargetAnimalComplete(int No)
    {
        //達成の演出

        //
        TargetUI[No].transform.Find("Content/Label_KilledUser").gameObject.SetActive(false);
        TargetUI[No].transform.Find("Content/Label_UserName").gameObject.SetActive(false);
        TargetUI[No].transform.Find("Content/ICON/Check").gameObject.SetActive(true);
    }

    public void TargetAnimalReset(int No)
    {
        //達成の演出

        //
        TargetUI[No].transform.Find("Content/Label_KilledUser").gameObject.SetActive(true);
        TargetUI[No].transform.Find("Content/Label_UserName").gameObject.SetActive(true);
        TargetUI[No].transform.Find("Content/ICON/Check").gameObject.SetActive(false);
    }


    public void ShowHitIndicator(bool isHeadshot)
    {
        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

       

        hitCoroutine = StartCoroutine(ShowHitCoroutine(isHeadshot));
    }
    private IEnumerator ShowHitCoroutine(bool isHeadshot)
    {
        HitImg.SetActive(false);
        HeadshotImg.SetActive(false);
        GameObject targetImg = isHeadshot ? HeadshotImg : HitImg;
        targetImg.SetActive(true);
        RectTransform rt = targetImg.GetComponent<RectTransform>();
        rt.localScale = Vector3.zero;
        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOScale(1.5f, 1f).SetEase(Ease.OutBack));
        yield return seq.WaitForCompletion();
        targetImg.SetActive(false);
    }

    public void ShowHeartshot(bool isHeartshot)
    {
        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);
        //ll
       

        hitCoroutine = StartCoroutine(ShowHeartshotCoroutine(isHeartshot));
    }
    private IEnumerator ShowHeartshotCoroutine(bool isHeartshot)
    {
        HitImg.SetActive(false);
        HeadshotImg.SetActive(false);
        GameObject targetImg = HitImg;
        targetImg.SetActive(true);
        RectTransform rt = targetImg.GetComponent<RectTransform>();
        rt.localScale = Vector3.zero;
        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOScale(1.5f,1f).SetEase(Ease.OutBack));
        yield return seq.WaitForCompletion();
        targetImg.SetActive(false);
    }




    public void ShowWaveHint(string content)
    {
        Debug.Log("ShowWaveHint: " + content);
        if (!string.IsNullOrEmpty(content))
        {
            waveHint.ShowWaveHint(content);
        }      
    }
    public void ShowPause()
    {
        enemyIndicator.PauseIndicator(true);
        if (currentPauseUI != null) return;
        currentPauseUI = Instantiate(PauseUI, uiParent);
        currentPauseUI.SetActive(true);

        Transform root = currentPauseUI.transform.Find("Root");
        if (root != null)
        {
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.localScale = Vector3.zero;
            rt.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack).SetUpdate(true)
             .OnComplete(() =>
             {
                 rt.DOScale(1f, 0.15f).SetEase(Ease.InOutSine)
                   .OnComplete(() =>
                   {
                       Time.timeScale = 0f;
                   });
             });
        }

    }
    public void ClosePause()
    {
        if (currentPauseUI != null)
        {
            Transform root = currentPauseUI.transform.Find("Root");
            if (root != null)
            {
                RectTransform rt = root.GetComponent<RectTransform>();         
                rt.DOScale(0f, 0.2f).SetEase(Ease.InBack).SetUpdate(true)
                  .OnComplete(() =>
                  {
                      Destroy(currentPauseUI);
                      currentPauseUI = null;

                      Time.timeScale = 1f;
                      enemyIndicator.PauseIndicator(false);
                  });
            }
            else
            {
                Destroy(currentPauseUI);
                currentPauseUI = null;
                Time.timeScale = 1f;
                enemyIndicator.PauseIndicator(false);
            }
        }
    }
    public void ShowQuit()
    {
        enemyIndicator.PauseIndicator(true);
        if (currentPauseUI != null)
        {
            Destroy(currentPauseUI);
            currentPauseUI = null;
        }
        currentQuitPauseUI = Instantiate(QuitpauseUI, uiParent);
        currentQuitPauseUI.SetActive(true);

        Transform root = currentQuitPauseUI.transform.Find("QuitPausee");
        if (root != null)
        {
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.localScale = Vector3.zero;


            rt.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack).SetUpdate(true)
              .OnComplete(() =>
              {
                  rt.DOScale(1f, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
              });
        }
    }

    public void Closequitpause()
    {   
        if (currentQuitPauseUI != null)
        {
            Transform root = currentQuitPauseUI.transform.Find("QuitPausee");
            if (root != null)
            {
                RectTransform rt = root.GetComponent<RectTransform>();

                rt.DOScale(0f, 0.2f).SetEase(Ease.InBack).SetUpdate(true)
                  .OnComplete(() =>
                  {
                      Destroy(currentQuitPauseUI);
                      currentQuitPauseUI = null;

                      Time.timeScale = 1f;
                      enemyIndicator.PauseIndicator(false);
                  });
            }
            else
            {
                Destroy(currentQuitPauseUI);
                currentQuitPauseUI = null;
                Time.timeScale = 1f;
                enemyIndicator.PauseIndicator(false);
            }
        }
    }

    public void QuitPauseUI()
    {
        Time.timeScale = 1f;
        //LifeManager.Instance.LoseLife();
        SceneManager.LoadScene(1);
    }
}




