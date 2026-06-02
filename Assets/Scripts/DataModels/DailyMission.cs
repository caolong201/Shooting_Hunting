using System;
using System.Collections.Generic;

[Serializable] 
public class MissionData
{
    public int ID;
    public string LIST;
    public int COUNT;
    public int STAR_COUNT;
    public int Reward;
    
    [NonSerialized] public MissionAction Action;
    [NonSerialized] public MissionKey Key;
}

[Serializable]
public class MissionProgress
{
    public int ID;
    public int Current;     // current progress (0..COUNT)
    public bool Completed;  // reached COUNT
    public bool Claimed;    // reward claimed
}

// Convenient runtime pair.
public sealed class MissionState
{
    public MissionData Def { get; }
    public MissionProgress Prog { get; }
    public MissionState(MissionData def, MissionProgress prog)
    {
        Def = def; Prog = prog;
    }
}

[Serializable]
class MissionProgressSaveBlob
{
    public int schemaVersion = 1;
    public List<MissionProgress> items = new List<MissionProgress>();
}

public interface IMissionStorage
{
    void Save(List<MissionProgress> progresses);
    List<MissionProgress> Load();
}

[Serializable]
public class MissionDataList
{
    public MissionData[] missions; 
}
