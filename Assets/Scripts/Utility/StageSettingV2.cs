using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageDataV2
{
    public int StageId;
    public string StageName;
    public int BulletCount;
    // public int Bear; 1
    // public int Deer; 2
    // public int Boar; 3
    // public int Wolf; 4
    // public int Tiger; 5
    // public int Moose; 6
    // public int RabbitBrown; 7
    // public int GoldRabbit; 8
    // public int BonusAnimal9; 9
    // public int BonusAnimal10; 10
    // public int BonusAnimal11; 11
    // public int BonusAnimal12; 12
    
    public List<WaveData> WaveData;
    public List<int> StageStarConditions;
}

[Serializable]
public class WaveData
{
    public string WaveName = "";
    public List<int> Animal;
    public Vector3 PlayerPosition;
    public Vector3 PlayerRoatation;

    public List<int> GenerateAnimalNo;
    public List<Vector3> GenerateAnimalPosition;
    public List<Vector3> GenerateAnimalRoatation;
    public List<int> GenerateAnimalAnimation;
    public ETargetAttack target = ETargetAttack.None;

}

[Serializable]
public enum ETargetAttack
{
    None = 0,
    Hunter = 1,
    Child = 2
}

[CreateAssetMenu(menuName = "ScriptableObject/Stage Setting V2", fileName = "StageSettingV2")]
public class StageSettingV2 : ScriptableObject
{
    public List<StageDataV2> DataList;
}
