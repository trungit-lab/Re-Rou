// FILE: LevelSpawnerScript.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpawnGroup
{
    public string description;
    public GameObject enemyPrefab;
    public int count;
    public AIProfile profileOverride;
    public float spawnDelay;
    public float delayBetweenSpawns;
    [Tooltip("Chỉ định mục tiêu cho nhóm quái này. -1 = không chỉ định. 0 = mục tiêu đầu tiên trong danh sách của Spawner, 1 = mục tiêu thứ hai,...")]
    public int objectiveIndex = -1; // <-- THÊM DÒNG NÀY
    
}

[System.Serializable]
public class Wave
{
    public string waveName;
    public List<SpawnGroup> spawnGroups;
    public float timeUntilNextWave = 60f;
}

[CreateAssetMenu(fileName = "New Level Spawner Script", menuName = "Spawner/Level Spawner Script")]
public class LevelSpawnerScript : ScriptableObject
{
    public List<Wave> waves;

}
