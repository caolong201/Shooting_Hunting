using System;
using System.Collections.Generic;
using UnityEngine;

public enum MissionAction
{
    None = 0,
    LoginDaily,
    UpgradeAmmo,
    WatchRewardedAds,
    CompleteStages,
    HitBrains,
    HitHearts,
    SpendMoney,        // amount-based
    SpinRoulette,
    PlayMinutes,       // time-based (minutes)
    CollectCoins,      // amount-based
    OpenTreasureBox,
    KillAnimals,
    KillRunningAnimals
}
public enum MissionKey
{
    Q1_LoginDaily = 1,
    Q2_UpgradeAmmo = 2,
    Q3_WatchRewardedAds = 3,
    Q4_Complete5Stages = 4,
    Q5_Complete10Stages = 5,
    Q6_Complete15Stages = 6,
    Q7_Hit5Brains = 7,
    Q8_Hit10Brains = 8,
    Q9_Hit15Brains = 9,
    Q10_Hit20Brains = 10,
    Q11_Hit5Hearts = 11,
    Q12_Hit10Hearts = 12,
    Q13_Hit15Hearts = 13,
    Q14_Hit20Hearts = 14,
    Q15_Spend20KMoney = 15,
    Q16_Spend35KMoney = 16,
    Q17_Spend50KMoney = 17,
    Q18_SpinRouletteOnce = 18,
    Q19_Play10Minutes = 19,
    Q20_Play20Minutes = 20,
    Q21_Play30Minutes = 21,
    Q22_Collect5KCoins = 22,
    Q23_Collect10KCoins = 23,
    Q24_Collect20KCoins = 24,
    Q25_OpenTreasureBoxOnce = 25,
    Q26_Kill10Animals = 26,
    Q27_Kill20Animals = 27,
    Q28_Kill30Animals = 28,
    Q29_Kill5RunningAnimals = 29,
    Q30_Kill10RunningAnimals = 30
}

public class MissionManager : SingletonMono<MissionManager>
{

    // Events (subscribe from your UI/game systems)
    public event Action<MissionState> OnMissionProgress;   // any progress change
    public event Action<MissionState> OnMissionCompleted;  // when becomes completed
    public event Action<MissionState> OnMissionClaimed;    // when reward claimed

    private readonly Dictionary<int, MissionData> _defs = new();
    public Dictionary<int, MissionData> Defs => _defs;
    
    private readonly Dictionary<int, MissionProgress> _progs = new();
    public Dictionary<int, MissionProgress> Progs => _progs;
    
    private IMissionStorage _storage;

    // For playtime accumulation (seconds to whole minutes).
    private double _playSecondsAccumulator = 0d;



  

    private void Awake()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Config/daily_mission");
        if (jsonFile != null)
        {
            string wrappedJson = "{\"missions\":" + jsonFile.text + "}";
            var defs = ParseDefinitionsFromRawArrayJson(wrappedJson);
            Debug.Log("Loaded missions: " + defs.Length);
            InitMission(defs);
        }
        
        OnDailyLogin();
    }



    void Update()
    {
        AddPlaySeconds(Time.deltaTime);
        if (Input.GetKeyDown(KeyCode.J))
        {
            AddPlaySeconds(30 * 60); // 30 phút = 1800 giây
            Debug.Log("Simulated 30 minutes played!");
        }
    }

    public void InitMission(IEnumerable<MissionData> definitions, IMissionStorage storage = null)
    {
        _storage = storage ?? new JsonFileMissionStorage();

        // Prepare definitions + derived fields.
        foreach (var def in definitions)
        {
            def.Key = (MissionKey)def.ID;
            def.Action = DeriveAction(def.ID, def.LIST);
            _defs[def.ID] = def;
        }

        // Load saved progress or initialize.
        var loaded = _storage.Load();
        if (loaded != null)
        {
            foreach (var p in loaded)
                if (_defs.ContainsKey(p.ID))
                    _progs[p.ID] = p;
        }
        foreach (var id in _defs.Keys)
        {
            if (!_progs.ContainsKey(id))
                _progs[id] = new MissionProgress { ID = id, Current = 0, Completed = false, Claimed = false };
        }
        // Normalize.
        foreach (var kv in _progs)
            ClampAndFix(kv.Value, _defs[kv.Key]);

        Save(); // persist normalized state
    }
    
    public static MissionData[] ParseDefinitionsFromRawArrayJson(string rawArrayJson)
    {
        var loadedData = JsonUtility.FromJson<MissionDataList>(rawArrayJson);
        return loadedData.missions;
    }

    #region Query API

    public IEnumerable<MissionState> GetAll()
    {
        foreach (var def in _defs.Values)
            yield return new MissionState(def, _progs[def.ID]);
    }

    public MissionState GetById(int id)
    {
        if (!_defs.ContainsKey(id)) return null;
        return new MissionState(_defs[id], _progs[id]);
    }
    public List<MissionState> GetMissions()
    {
        return new List<MissionState>(GetAll());
    }
    public MissionState GetByKey(MissionKey key) => GetById((int)key);

    public int GetTotalStarsClaimed()
    {
        int stars = 0;
        foreach (var def in _defs.Values)
        {
            var p = _progs[def.ID];
            if (p.Claimed) stars += def.STAR_COUNT;
        }
        return stars;
    }

    #endregion

    #region Claim / Reset

    // Claim reward if completed and not yet claimed.
    public bool TryClaim(int id, out int reward, out int stars)
    {
        reward = 0; stars = 0;
        if (!_defs.TryGetValue(id, out var def)) return false;
        if (!_progs.TryGetValue(id, out var prog)) return false;
        if (!prog.Completed || prog.Claimed) return false;

        prog.Claimed = true;
        reward = def.Reward;
        stars = def.STAR_COUNT;
        Save();
        return true;
    }

    // Reset all missions (e.g., for debug/testing).
    public void ResetAll()
    {
        foreach (var p in _progs.Values)
        {
            p.Current = 0; p.Completed = false; p.Claimed = false;
        }
        Save();
    }

    // Reset a single mission by ID/Key.
    public bool ResetOne(int id)
    {
        if (!_progs.TryGetValue(id, out var p) || !_defs.TryGetValue(id, out var d)) return false;
        p.Current = 0; p.Completed = false; p.Claimed = false;
        Save();
        OnMissionProgress?.Invoke(new MissionState(d, p));
        return true;
    }

    public bool ResetOne(MissionKey key) => ResetOne((int)key);

    #endregion

    #region Precise per-mission increment (1–1)

    // Increment exactly one mission by ID/Key.
    public bool AddProgressByMissionId(int id, int amount = 1)
    {
        if (!_defs.TryGetValue(id, out var def)) return false;
        return AddProgressByMissionKey(def.Key, amount);
    }

    public bool AddProgressByMissionKey(MissionKey key, int amount = 1)
    {
        if (amount <= 0) return false;
        int id = (int)key;
        if (!_defs.TryGetValue(id, out var def)) return false;
        var prog = _progs[id];
        if (prog.Completed) return false;

        int before = prog.Current;
        prog.Current = Mathf.Clamp(prog.Current + amount, 0, def.COUNT);

        bool wasCompleted = prog.Completed;
        prog.Completed = prog.Current >= def.COUNT;

        bool changed = (prog.Current != before) || (prog.Completed != wasCompleted);
        if (changed)
        {
            Save();
            var state = new MissionState(def, prog);
            OnMissionProgress?.Invoke(state);
            if (!wasCompleted && prog.Completed)
                OnMissionCompleted?.Invoke(state);
        }
        return changed;
    }

    #endregion

    #region Generic gameplay hooks (advance all missions of a category)

    public void OnDailyLogin() => IncrementCategory(MissionAction.LoginDaily, 1);                             
    public void OnAmmoUpgraded(int upgrades = 1) => IncrementCategory(MissionAction.UpgradeAmmo, upgrades);     
    public void OnRewardedAdWatched() => IncrementCategory(MissionAction.WatchRewardedAds, 1);                  

    public void OnStageCompleted(int stages = 1) => IncrementCategory(MissionAction.CompleteStages, stages);    

    public void OnBrainHit(int hits = 1) => IncrementCategory(MissionAction.HitBrains, hits);                   
    public void OnHeartHit(int hits = 1) => IncrementCategory(MissionAction.HitHearts, hits);                       

    // Same unit as COUNT in your JSON (e.g., coins or money units).
    public void OnMoneySpent(int amount) => IncrementCategory(MissionAction.SpendMoney, amount);                        

    public void OnRouletteSpun() => IncrementCategory(MissionAction.SpinRoulette, 1);                               

    // Accumulate coin collection missions.
    public void OnCoinsCollected(int amount) => IncrementCategory(MissionAction.CollectCoins, amount);               
    public void OnTreasureBoxOpened() => IncrementCategory(MissionAction.OpenTreasureBox, 1);                      

    public void OnAnimalKilled(int count = 1) => IncrementCategory(MissionAction.KillAnimals, count);             

    public void OnRunningAnimalKilled(int count = 1) => IncrementCategory(MissionAction.KillRunningAnimals, count);            

    // Call each frame or fixed step with deltaTime seconds.
    public void AddPlaySeconds(double deltaSeconds)
    {
        if (deltaSeconds <= 0) return;
        _playSecondsAccumulator += deltaSeconds;

        // Convert to whole minutes to avoid float jitter.
        var wholeMinutes = (int)(_playSecondsAccumulator / 60.0);
        if (wholeMinutes > 0)
        {
            _playSecondsAccumulator -= wholeMinutes * 60.0;
            IncrementCategory(MissionAction.PlayMinutes, wholeMinutes);
        }
    }
    
    public bool HasClaimableMissions()
    {
        foreach (var def in _defs.Values)
        {
            var p = _progs[def.ID];
            if (p.Completed && !p.Claimed) return true;
        }
        return false;
    }

    #endregion

    #region Core increment logic

    private int _currentBatchIndex = 0;
    private const int MissionsPerBatch = 5;

    public void SetCurrentBatchIndex(int index)
    {
        _currentBatchIndex = index;
    }
    private void IncrementCategory(MissionAction action, int amount)
    {
        if (amount <= 0) return;

        bool changedAny = false;

       
        int start = _currentBatchIndex * MissionsPerBatch + 1;
        int end = Mathf.Min(start + MissionsPerBatch - 1, _defs.Count);

        foreach (var def in _defs.Values)
        {
           
            if (def.ID < start || def.ID > end) continue;
            if (def.Action != action) continue;

            var prog = _progs[def.ID];
            if (prog.Completed) continue;

            int before = prog.Current;
            prog.Current = Mathf.Clamp(prog.Current + amount, 0, def.COUNT);

            bool wasCompleted = prog.Completed;
            prog.Completed = prog.Current >= def.COUNT;

            if (prog.Current != before || prog.Completed != wasCompleted)
            {
                changedAny = true;
                var state = new MissionState(def, prog);
                OnMissionProgress?.Invoke(state);
                if (!wasCompleted && prog.Completed)
                    OnMissionCompleted?.Invoke(state);
            }
        }

        if (changedAny) Save();
    }
   

    //private void IncrementCategory(MissionAction action, int amount)
    //{
    //    if (amount <= 0) return;

    //    bool changedAny = false;

    //    foreach (var def in _defs.Values)
    //    {
    //        if (def.Action != action) continue;
    //        var prog = _progs[def.ID];

    //        if (prog.Completed) continue;

    //        int before = prog.Current;
    //        prog.Current = Mathf.Clamp(prog.Current + amount, 0, def.COUNT);

    //        bool wasCompleted = prog.Completed;
    //        prog.Completed = prog.Current >= def.COUNT;

    //        if (prog.Current != before || prog.Completed != wasCompleted)
    //        {
    //            changedAny = true;
    //            var state = new MissionState(def, prog);
    //            OnMissionProgress?.Invoke(state);
    //            if (!wasCompleted && prog.Completed)
    //                OnMissionCompleted?.Invoke(state);
    //        }
    //    }

    //    if (changedAny) Save();
    //}

    private void ClampAndFix(MissionProgress p, MissionData d)
    {
        if (p == null || d == null) return;
        p.Current = Mathf.Clamp(p.Current, 0, d.COUNT);
        p.Completed = p.Current >= d.COUNT || p.Completed;
        if (!p.Completed) p.Claimed = false; // cannot be claimed unless completed
    }

    private void Save() => _storage.Save(new List<MissionProgress>(_progs.Values));

    #endregion

    #region Helpers — mapping LIST/ID to MissionAction

    // Map strictly by ID according to your provided 30 missions.
    private static MissionAction DeriveAction(int id, string list)
    {
        switch (id)
        {
            case 1:  return MissionAction.LoginDaily;
            case 2:  return MissionAction.UpgradeAmmo;
            case 3:  return MissionAction.WatchRewardedAds;

            case 4:
            case 5:
            case 6:  return MissionAction.CompleteStages;

            case 7:
            case 8:
            case 9:
            case 10: return MissionAction.HitBrains;

            case 11:
            case 12:
            case 13:
            case 14: return MissionAction.HitHearts;

            case 15:
            case 16:
            case 17: return MissionAction.SpendMoney;

            case 18: return MissionAction.SpinRoulette;

            case 19:
            case 20:
            case 21: return MissionAction.PlayMinutes;

            case 22:
            case 23:
            case 24: return MissionAction.CollectCoins;

            case 25: return MissionAction.OpenTreasureBox;

            case 26:
            case 27:
            case 28: return MissionAction.KillAnimals;

            case 29:
            case 30: return MissionAction.KillRunningAnimals;

            default: return MissionAction.None;
        }
    }

    #endregion
}
