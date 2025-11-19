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
    public int objectiveIndex = -1; 
    
}

[System.Serializable]
public class Wave
{
    public string waveName;
    public List<SpawnGroup> spawnGroups;
    public float timeUntilNextWave = 60f;
}

public enum GameMode
{
    Annihilation, // Chế độ Tiêu Diệt
    TimedSurvival // Chế độ Sinh Tồn
}

[CreateAssetMenu(fileName = "New Level Spawner Script", menuName = "Spawner/Level Spawner Script")]
public class LevelSpawnerScript : ScriptableObject
{
    [Header("Game Mode Settings")]
    [Tooltip("Chọn chế độ chơi cho kịch bản này.")]
    public GameMode gameMode;

    [Tooltip("Chỉ dùng cho TimedSurvival. Thời gian người chơi phải sống sót ở sóng CUỐI CÙNG để thắng.")]
    public float finalWaveSurvivalTime = 180f; // 3 phút

    [Header("Wave Configuration")]
    [Tooltip("Danh sách tất cả các sóng sẽ xuất hiện trong màn chơi này.")]
    public List<Wave> waves;
}