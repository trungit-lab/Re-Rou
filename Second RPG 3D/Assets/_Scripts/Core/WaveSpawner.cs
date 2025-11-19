// FILE: WaveSpawner.cs (Phiên bản cuối cùng, hỗ trợ cả 2 chế độ chơi)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Tooltip("Kéo file 'Kịch Bản' (LevelSpawnerScript) của màn chơi vào đây.")]
    public LevelSpawnerScript spawnerScript;

    [Tooltip("Danh sách các điểm mà quái có thể xuất hiện.")]
    public Transform[] spawnPoints;

    [Tooltip("Danh sách các mục tiêu chính mà quái có thể tấn công.")]
    public List<Transform> mainObjectives;

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private bool isSpawningWave = false; // Biến cờ quan trọng cho chế độ Annihilation

    void Start()
    {
        if (spawnerScript == null || spawnerScript.waves.Count == 0)
        {
            Debug.LogError("Spawner Script chưa được gán hoặc không có sóng nào!");
            return;
        }

        // Dựa vào chế độ chơi trong kịch bản để gọi Coroutine tương ứng
        if (spawnerScript.gameMode == GameMode.Annihilation)
        {
            StartCoroutine(AnnihilationFlowCoroutine());
        }
        else // TimedSurvival
        {
            StartCoroutine(TimedSurvivalFlowCoroutine());
        }
    }

    // --- LOGIC CHO CHẾ ĐỘ TIÊU DIỆT (GIẾT HẾT) ---
    private IEnumerator AnnihilationFlowCoroutine()
    {
        Debug.Log("Bắt đầu chế độ chơi: Annihilation");
        for (currentWaveIndex = 0; currentWaveIndex < spawnerScript.waves.Count; currentWaveIndex++)
        {
            Wave currentWave = spawnerScript.waves[currentWaveIndex];
            GameManager.Instance.UpdateWaveUI($"{currentWaveIndex + 1}/{spawnerScript.waves.Count}");

            yield return StartCoroutine(SpawnWaveCoroutine(currentWave));

            // Chờ cho đến khi tất cả quái vật của sóng này bị tiêu diệt
            while (enemiesAlive > 0)
            {
                yield return null;
            }

            Debug.Log("Đã hoàn thành sóng: " + currentWave.waveName);
            if (currentWaveIndex < spawnerScript.waves.Count - 1)
            {
                yield return new WaitForSeconds(3f); // Thời gian nghỉ giữa các sóng
            }
        }
        Debug.Log("CHIẾN THẮNG! (Annihilation)");
        GameManager.Instance.ChangeGameState(GameState.WIN);
    }

    // --- LOGIC CHO CHẾ ĐỘ SINH TỒN (THEO THỜI GIAN) ---
    private IEnumerator TimedSurvivalFlowCoroutine()
    {
        Debug.Log("Bắt đầu chế độ chơi: Timed Survival");
        for (currentWaveIndex = 0; currentWaveIndex < spawnerScript.waves.Count; currentWaveIndex++)
        {
            Wave currentWave = spawnerScript.waves[currentWaveIndex];

            StartCoroutine(SpawnWaveCoroutine(currentWave));

            float countdown = (currentWaveIndex == spawnerScript.waves.Count - 1)
                ? spawnerScript.finalWaveSurvivalTime
                : currentWave.timeUntilNextWave;

            while (countdown > 0)
            {
                string message = (currentWaveIndex == spawnerScript.waves.Count - 1)
                    ? $"SỐNG SÓT!"
                    : $"Sóng {currentWaveIndex + 1}/{spawnerScript.waves.Count}";
                GameManager.Instance.UpdateWaveUI(message, countdown);
                countdown -= Time.deltaTime;
                yield return null;
            }
        }
        Debug.Log("CHIẾN THẮNG! (TimedSurvival)");
        GameManager.Instance.UpdateWaveUI("ĐÃ SỐNG SÓT!", -1);
        GameManager.Instance.ChangeGameState(GameState.WIN);
    }

    // --- CÁC HÀM HỖ TRỢ (DÙNG CHUNG CHO CẢ 2 CHẾ ĐỘ) ---

    // Chịu trách nhiệm sinh ra TẤT CẢ quái của MỘT sóng
    private IEnumerator SpawnWaveCoroutine(Wave wave)
    {
        isSpawningWave = true;

        int enemiesToSpawnInThisWave = 0;
        foreach (var group in wave.spawnGroups)
        {
            enemiesToSpawnInThisWave += group.count;
        }

        enemiesAlive += enemiesToSpawnInThisWave;
        GameManager.Instance.OnWaveStarted(enemiesAlive);

        foreach (var group in wave.spawnGroups)
        {
            StartCoroutine(SpawnGroupCoroutine(group));
        }

        // Đợi một chút để đảm bảo coroutine cuối cùng được khởi chạy
        yield return new WaitForEndOfFrame();
        isSpawningWave = false;
    }

    // Chịu trách nhiệm sinh ra MỘT NHÓM quái
    private IEnumerator SpawnGroupCoroutine(SpawnGroup group)
    {
        yield return new WaitForSeconds(group.spawnDelay);
        for (int i = 0; i < group.count; i++)
        {
            if (spawnPoints.Length == 0) { yield break; }
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyGO = ObjectPooler.Instance.SpawnFromPool(group.enemyPrefab.name, spawnPoint.position, spawnPoint.rotation);

            if (enemyGO != null)
            {
                BaseAIController aiController = enemyGO.GetComponentInChildren<BaseAIController>();
                if (aiController != null)
                {
                    aiController.ApplyProfile(group.profileOverride);
                    if (group.objectiveIndex >= 0 && mainObjectives.Count > group.objectiveIndex)
                    {
                        aiController.mainObjectiveTarget = mainObjectives[group.objectiveIndex];
                    }
                }
            }
            yield return new WaitForSeconds(group.delayBetweenSpawns);
        }
    }

    // Hàm này được GameManager gọi khi một kẻ địch bị tiêu diệt
    public void OnAnEnemyWasKilled()
    {
        enemiesAlive--;
        // Cập nhật lại UI tổng số quái đang sống
        GameManager.Instance.OnWaveStarted(enemiesAlive);
    }
}