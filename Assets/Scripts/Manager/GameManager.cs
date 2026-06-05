using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using DG.Tweening;
using TMPro;
using IE.RSB;
using Unity.Cinemachine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private float countDown = 20.0f;
    public UIManager uIManager;

    //クリックされたかどうか
    private bool inGameBool = false;
    [SerializeField] private TouchCameraRotation m_touchCameraRotation = null;
    [SerializeField] private PlayerWeaponController m_weaponController = null;
    [SerializeField] private MotionController m_weaponMotionController = null;

    //[SerializeField]private StageSetting m_stageSetting;
    [SerializeField] private StageSettingV2 m_stageSetting;
    [SerializeField] private GameObject player;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private List<GameObject> animals = new List<GameObject>();
    [SerializeField] private Button nextBtn;

    private int stageNo = 1; //unlimit
    private int stageData = 1; //limit max stage
    private int waveNo = 1;

    public int targetAnimal = 0;
    public List<int> AnimalCount = new List<int>();

    private bool clearStatus = false;
    [SerializeField] private ArrowIndicatorManager arrowManager;
    [SerializeField] private EnemyIndicator enemyIndicator;
    [SerializeField] private int maxStage = 3;


    [SerializeField] private GameObject childPrefab;
    [SerializeField] private GameObject currentChild;

    private StageDataV2 _currStageData = null;
    private WaveData _currWaveData = null;

    [Header("DEBUG")] [SerializeField] private bool _isDebugStage = false;
    [SerializeField] private int _stage = 1;
    [SerializeField] private int _wave = 1;

    public bool isFailed = false;
    private static bool hasLoaded = false;
    [SerializeField] DogNavAgentController huntingDogPrefab;
    [SerializeField] private FollowDogCamera camHuntingDog;
    [SerializeField] CanvasGroup canvasGroupUI;

    void Awake()
    {
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        //ステージ生成
        nextBtn.onClick.AddListener(() => OnClickStageNext());
        StageGenerate();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            OnClickHuntingDog();
        }

        if (!inGameBool) return;

        //時間をカウントダウンする
        countDown -= Time.deltaTime;

        if (countDown < 0)
        {
            countDown = 0f;
            inGameBool = false;
        }
    }

    void LateUpdate()
    {
        if (SniperAndBallisticsSystem.instance.BulletTimeRunning)
        {
            enemyIndicator.closeIndicator();
            return;
        }

        enemyIndicator.OpenIndicator();

        if (m_weaponController.ammoZeroFlg)
        {
            m_weaponController.ammoZeroFlg = false;
            if (targetAnimal > 0)
            {
                //Debug.Log("ここまできたTest1");
                Faild();
            }
        }

        if (clearStatus)
        {
            clearStatus = false;
            StageClear();
        }
    }


    void StageGenerate()
    {
        isFailed = false;
        if (_isDebugStage)
        {
            stageNo = _stage;
            waveNo = _wave;
            stageData = stageNo;
            _isDebugStage = false;
        }
        else
        {
            int playStage = PlayerPrefs.GetInt(PopupLVManager.PlayStageNoKey, 0);
            if (playStage > 0)
            {
                stageNo = playStage;
                stageData = playStage;
                PlayerPrefs.DeleteKey(PopupLVManager.PlayStageNoKey);
                PlayerPrefs.Save();
            }
            else
            {
                stageNo = PlayerPrefs.GetInt("StageNo", 1);
                stageData = stageNo;
            }

            waveNo = 1;

            if (stageData > maxStage)
            {
                stageData = UnityEngine.Random.Range(2, maxStage + 1);
            }
        }

        GameAnalyticsManager.Instance.TrackEvent($"Stage{stageNo}:WholeProgress:Level:start");
        uIManager.SetStageName(("Stage" + stageNo));


        _currStageData = m_stageSetting.DataList.FirstOrDefault(stage => stage.StageId == stageData);
        var waveData = _currStageData.WaveData[waveNo - 1];
        //弾数をセット
        uIManager.SetBulletCount(_currStageData.BulletCount);

        //プレイヤーのセッティング
        Vector3 playerPos = waveData.PlayerPosition;
        //Debug.Log("PlayPos:"+playerPos);
        player.transform.position = playerPos;
        player.transform.rotation = Quaternion.Euler(waveData.PlayerRoatation);
        m_touchCameraRotation.ResetRotation(false);


        //ターゲットアニマル生成
        foreach (Transform child in enemyParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < waveData.GenerateAnimalNo.Count; i++)
        {
            int no = waveData.GenerateAnimalNo[i] - 1;

            Vector3 spawnPosition = waveData.GenerateAnimalPosition[i];
            Quaternion spawnRotation = Quaternion.Euler(waveData.GenerateAnimalRoatation[i]);

            GameObject newObject = Instantiate(animals[no], spawnPosition, spawnRotation, enemyParent);


            if (waveData.GenerateAnimalAnimation[i] > 0)
            {
                newObject.GetComponent<Enemy>().DoMoveAnimal(waveData.GenerateAnimalAnimation[i]);
            }

            if (waveData.target == ETargetAttack.Hunter)
            {
                newObject.GetComponent<Enemy>().SetTargetAttack(player.transform, 5);
            }
            else if (waveData.target == ETargetAttack.Child)
            {
                if (currentChild == null)
                    currentChild = Instantiate(childPrefab, enemyParent);

                if (i == 0) newObject.GetComponent<Enemy>().SetTargetAttack(currentChild.transform, 2, i * 4);
            }
        }

        List<Enemy> enemyList = new List<Enemy>();
        foreach (Transform child in enemyParent)
        {
            Enemy enemy = child.GetComponent<Enemy>();
            if (enemy != null) enemyList.Add(enemy);
        }

        List<Enemy> filteredEnemies = enemyList.Where(e => e != null && e.animalNo != 8).ToList();
        arrowManager.SetEnemies(filteredEnemies);
        enemyIndicator.SetEnemies(enemyList);

        targetAnimal = 0;
        AnimalCount = new List<int>();
        //ターゲット情報のUI更新
        for (int i = 0; i < waveData.Animal.Count; i++)
        {
            if (i == 7) continue;

            targetAnimal += waveData.Animal[i];
            AnimalCount.Add(waveData.Animal[i]);
            //Bear
            if (waveData.Animal[i] > 0)
            {
                //テキスト更新
                uIManager.TargetUI[i].transform.Find("Content/Label_KilledUser").gameObject
                    .GetComponent<TextMeshProUGUI>().text = "<color=#EC6161>" + waveData.Animal[i].ToString();
                //テキスト表示
                uIManager.TargetUI[i].SetActive(true);
            }
            else
            {
                if (uIManager.TargetUI[i].activeSelf) uIManager.TargetUI[i].SetActive(false);
            }

            uIManager.TargetAnimalReset(i);
        }

        //UIリセット
        uIManager.HUDView();

        //クリアテキスト
        for (int i = 0; i < _currStageData.StageStarConditions.Count; i++)
        {
            uIManager.ClearText(i, _currStageData.StageStarConditions[i]);
        }

        DOVirtual.DelayedCall(2, () => { uIManager.ShowWaveHint(_currStageData.WaveData[waveNo - 1].WaveName); });

        StartCoroutine(Complete());
    }

    void WaveGenerate()
    {
        isFailed = false;
        //StageNameセット
        uIManager.SetStageName(("Stage" + stageNo));

        var waveData = _currStageData.WaveData[waveNo - 1];
        //弾数をセット
        uIManager.SetBulletCount(_currStageData.BulletCount);

        //ターゲットアニマル生成
        foreach (Transform child in enemyParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < waveData.GenerateAnimalNo.Count; i++)
        {
            int no = waveData.GenerateAnimalNo[i] - 1;

            Vector3 spawnPosition = waveData.GenerateAnimalPosition[i];
            Quaternion spawnRotation = Quaternion.Euler(waveData.GenerateAnimalRoatation[i]);

            GameObject newObject = Instantiate(animals[no], spawnPosition, spawnRotation, enemyParent);
            if (waveData.GenerateAnimalAnimation[i] > 0)
            {
                newObject.GetComponent<Enemy>().DoMoveAnimal(waveData.GenerateAnimalAnimation[i]);
            }

            if (waveData.target == ETargetAttack.Hunter)
            {
                newObject.GetComponent<Enemy>().SetTargetAttack(player.transform, 5);
            }
        }

        List<Enemy> enemyList = new List<Enemy>();
        foreach (Transform child in enemyParent)
        {
            Enemy enemy = child.GetComponent<Enemy>();
            if (enemy != null) enemyList.Add(enemy);
        }

        List<Enemy> filteredEnemies = enemyList.Where(e => e != null && e.animalNo != 8).ToList();
        arrowManager.SetEnemies(filteredEnemies);
        enemyIndicator.SetEnemies(enemyList);

        targetAnimal = 0;
        AnimalCount = new List<int>();
        //ターゲット情報のUI更新
        for (int i = 0; i < waveData.Animal.Count; i++)
        {
            if (i == 7) continue;
            targetAnimal += waveData.Animal[i];
            AnimalCount.Add(waveData.Animal[i]);
            //Bear
            if (waveData.Animal[i] > 0)
            {
                //テキスト更新
                uIManager.TargetUI[i].transform.Find("Content/Label_KilledUser").gameObject
                    .GetComponent<TextMeshProUGUI>().text = "<color=#EC6161>" + waveData.Animal[i].ToString();
                //テキスト表示
                uIManager.TargetUI[i].SetActive(true);
            }
            else
            {
                if (uIManager.TargetUI[i].activeSelf) uIManager.TargetUI[i].SetActive(false);
            }

            uIManager.TargetAnimalReset(i);
        }

        inGameBool = true;
        m_touchCameraRotation.touchFlg = true;
    }

    public void EnemyDown(int No)
    {
        No -= 1;

        if (No == 7) return;

        targetAnimal -= 1;
        AnimalCount[No] -= 1;

        uIManager.TargetUI[No].transform.Find("Content/Label_KilledUser").gameObject.GetComponent<TextMeshProUGUI>()
            .text = "<color=#EC6161>" + AnimalCount[No].ToString();
        if (AnimalCount[No] <= 0)
        {
            EnemyCountZero(No);
        }

        //全ての動物がいなくなったか？チェック
        if (targetAnimal <= 0)
        {
            clearStatus = !clearStatus;
        }
    }

    //対象動物の数が0になった際の処理
    void EnemyCountZero(int No)
    {
        uIManager.TargetAnimalComplete(No);
    }

    IEnumerator Complete()
    {
        //yield return new WaitForSeconds(3);
        uIManager.LoadingComplete();
        yield return new WaitForSeconds(1);
        inGameBool = true;
        m_touchCameraRotation.touchFlg = true;
    }

    void StageClear()
    {
        if (SniperAndBallisticsSystem.instance.BulletTimeRunning) return;
        m_touchCameraRotation.touchFlg = false;
        isFailed = false;

        m_weaponController.scopeMode = false;
        StartCoroutine(m_touchCameraRotation.WaiteScopeChange(0, true));
        canvasGroupUI.alpha = 1;

        if (waveNo < _currStageData.WaveData.Count)
        {
            //next wave
            waveNo++;
            Debug.Log("next wave: " + waveNo);
            DOVirtual.DelayedCall(1, () =>
            {
                m_touchCameraRotation.ResetRotation(true);
                uIManager.ShowWaveHint(_currStageData.WaveData[waveNo - 1].WaveName);

                player.GetComponent<TransitionWave>().StartTransition(
                    _currStageData.WaveData[waveNo - 1].PlayerPosition,
                    _currStageData.WaveData[waveNo - 1].PlayerRoatation,
                    () => { WaveGenerate(); });
            });
        }
        else
        {
            //next stage
            Debug.Log("next stage");
            PlayerPrefs.SetInt("StageNo", stageNo + 1);
            PlayerPrefs.Save();
            StartCoroutine(ClearEchoes());
        }
    }


    //クリア演出の余韻が欲しい
    IEnumerator ClearEchoes()
    {
        //クリア状態
        //残り玉数と条件の比較
        int bullet = m_weaponController.m_availableAmmoNow;
        var stageData = m_stageSetting.DataList.FirstOrDefault(stage => stage.StageId == this.stageData);

        for (int i = 0; i < stageData.StageStarConditions.Count; i++)
        {
            if (bullet >= stageData.StageStarConditions[i])
            {
                //Debug.Log("Clear"+i);
                uIManager.ClearStatusUIChange(i);
            }
        }

        yield return new WaitForSeconds(1);
        uIManager.ClearUIChange();
        enemyIndicator.SetGameClear(true);
    }

    public void Faild()
    {
        enemyIndicator.PauseIndicator(true);
        //enemyIndicator.closeIndicator();
        if (SniperAndBallisticsSystem.instance.BulletTimeRunning) return;
        m_touchCameraRotation.touchFlg = false;
        //uIManager.FaildUIChange();
        //isFailed = true;
        if (!isFailed)
        {
            uIManager.FaildUIChange();
            isFailed = true;
        }
    }

    public void OnClickStageRetry(bool clearStatus)
    {
        //Debug.Log("リスタート！");
        uIManager.LoadingStart();
        enemyIndicator.SetGameClear(false);
        enemyIndicator.OpenIndicator();
        if (clearStatus)
        {
            uIManager.ClearUIChange();
            PlayerPrefs.SetInt("StageNo", stageNo);
        }
        else
        {
            currentChild = null;
            uIManager.FaildUIChange();

            if (LifeManager.Instance.CurrentLifes <= 0)
            {
                Debug.Log("out of live -> back to home");
                SceneManager.LoadScene(1);
            }
            else
            {
                LifeManager.Instance.LoseLife();
            }
        }

        StageGenerate();
    }

    public void OnClickStageRevive()
    {
        AdManager.Instance.ShowRewardedAd((b) =>
        {
            if (b)
            {
                isFailed = false;
                enemyIndicator.OpenIndicator();
                uIManager.SetBulletCount(_currStageData.BulletCount);
                inGameBool = true;
                m_touchCameraRotation.touchFlg = true;
                uIManager.FaildUIChange();
                uIManager.HUDView();

                if (currentChild != null)
                {
                    currentChild.GetComponent<Animator>().Play("Idle");
                }
            }
        });
    }

    public void OnClickStageNext()
    {
        //Debug.Log("次のステージ");
        uIManager.LoadingStart();
        enemyIndicator.SetGameClear(false);
        uIManager.ClearUIClose();
        //セーブ
        //StageGenerate();
        if (stageNo == 1 && !hasLoaded)
        {
            hasLoaded = true;
            SceneManager.LoadScene(1);
        }
        else
        {
            PlayerPrefs.SetInt("StageNo", stageNo + 1);
            PlayerPrefs.Save();
            StageGenerate();
        }
    }

    public void OnbtShowPause()
    {
        uIManager.ShowPause();
    }

    public void OnbtContinuePause()
    {
        uIManager.ClosePause();
    }

    public void OnbtShowQuit()
    {
        uIManager.ShowQuit();
    }

    public void OnbtQuitPause()
    {
        uIManager.Closequitpause();
    }

    public void OnbtQuitgame()
    {
        uIManager.QuitPauseUI();
    }


    public void OnClickHuntingDog()
    {
        Vector3 pos = player.transform.position;

        Transform target = null;
        foreach (Transform child in enemyParent)
        {
            Enemy enemy = child.GetComponent<Enemy>();
            if (enemy != null && !enemy.isDead)
            {
                target = enemy.transform;
                break;
            }
        }

        for (int i = 0; i < 2; i++)
        {
            if (i == 0)
            {
                pos = new Vector3(pos.x, pos.y, pos.z);
            }
            else
            {
                pos = new Vector3(pos.x, pos.y, pos.z);
            }

            DogNavAgentController dog = Instantiate(huntingDogPrefab, pos, Quaternion.identity, enemyParent);
            dog.Attack(target, () =>
            {
                StartCoroutine(camHuntingDog.ReturnHome(() =>
                {
                    camHuntingDog.gameObject.SetActive(false);
                    canvasGroupUI.alpha = 1;
                }));
            });

            if (i == 0)
            {
                camHuntingDog.ActivateFollow(dog.transform);
                camHuntingDog.gameObject.SetActive(true);
                canvasGroupUI.alpha = 0;
            }
        }
    }
}