using System.Collections.Generic;
using UnityEngine;

public class EnemyPreviewManager : MonoBehaviour
{

    public List<GameObject> enemyPrefabs;   
    public Transform spawnPoint;
    private GameObject currentEnemy;

    void Start()
    {
        int currentStage = PlayerPrefs.GetInt("StageNo", 1);
        ShowEnemyForStage(currentStage);
    }

    public void ShowEnemyForStage(int stageNumber)
    {
     
        if (currentEnemy != null)
        {
            Destroy(currentEnemy);
        }

        GameObject prefabToSpawn = null;

        if (stageNumber <= enemyPrefabs.Count && stageNumber <= 10)
        {
           
            prefabToSpawn = enemyPrefabs[stageNumber - 1];
        }
        else
        {
            int randomIndex = Random.Range(0, enemyPrefabs.Count);
            prefabToSpawn = enemyPrefabs[randomIndex];
        }

        if (prefabToSpawn != null)
        {
            currentEnemy = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation, spawnPoint);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Prefab Enemy cho Stage " + stageNumber);
        }
    }
}
