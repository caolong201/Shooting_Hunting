using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public sealed class JsonFileMissionStorage : IMissionStorage
{
    private readonly string _path;

    public JsonFileMissionStorage(string fileName = "missions_progress.json")
    {
        _path = Path.Combine(Application.persistentDataPath, fileName);
    }

    public void Save(List<MissionProgress> progresses)
    {
        var blob = new MissionProgressSaveBlob { items = progresses };
        var json = JsonUtility.ToJson(blob, prettyPrint: false);
        File.WriteAllText(_path, json);
#if UNITY_EDITOR
        Debug.Log($"[MissionStorage] Saved -> {_path}");
#endif
    }

    public List<MissionProgress> Load()
    {
        if (!File.Exists(_path)) return null;
        try
        {
            var json = File.ReadAllText(_path);
            var blob = JsonUtility.FromJson<MissionProgressSaveBlob>(json);
            return blob?.items ?? new List<MissionProgress>();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MissionStorage] Load failed: {e.Message}");
            return null;
        }
    }
}