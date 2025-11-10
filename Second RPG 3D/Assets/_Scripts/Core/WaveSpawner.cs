// FILE: WaveSpawner.cs (Phiên bản đã sửa lỗi)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Tooltip("Kéo file 'Kịch Bản' (LevelSpawnerScript) của màn chơi vào đây.")]
    public LevelSpawnerScript spawnerScript;

    [Tooltip("Danh sách các điểm mà quái có thể xuất hiện.")]
    public Transform[] spawnPoints;

    [Tooltip("Danh sách các mục tiêu chính (ví dụ: cổng, nhà chính) mà quái có thể tấn công.")]
    public List<Transform> mainObjectives;

    private int currentWaveIndex = 0;
    private int enemiesAliveInWave = 0;
    

    void Start()
    {
        if (spawnerScript == null || spawnerScript.waves.Count == 0)
        {
            Debug.LogError("Spawner Script chưa được gán hoặc không có sóng nào!");
            return;
        }
        // Bắt đầu Coroutine chính quản lý toàn bộ màn chơi
        StartCoroutine(LevelFlowCoroutine());
    }

    // Coroutine chính điều khiển luồng của cả màn chơi
    private IEnumerator LevelFlowCoroutine()
    {
        // Lặp qua từng sóng trong kịch bản
        for (currentWaveIndex = 0; currentWaveIndex < spawnerScript.waves.Count; currentWaveIndex++)
        {
            Wave currentWave = spawnerScript.waves[currentWaveIndex];

            // Bắt đầu sinh ra quái của sóng hiện tại (không cần chờ nó kết thúc)
            StartCoroutine(SpawnWaveCoroutine(currentWave));

            float countdown = currentWave.timeUntilNextWave;

            // Bắt đầu đếm ngược đến sóng tiếp theo
            while (countdown > 0)
            {
                // Cập nhật UI mỗi frame
                GameManager.Instance.UpdateWaveUI(currentWaveIndex + 1, spawnerScript.waves.Count, countdown);

                countdown -= Time.deltaTime;
                yield return null; // Chờ đến frame tiếp theo
            }
        }

        // Sau khi vòng lặp kết thúc (tức là bộ đếm của sóng cuối cùng đã hết)
        // -> Người chơi đã sống sót!
        Debug.Log("CHIẾN THẮNG! Đã sống sót qua tất cả các sóng!");
        GameManager.Instance.UpdateWaveUI(currentWaveIndex, spawnerScript.waves.Count, 0); // Cập nhật UI lần cuối
        GameManager.Instance.ChangeGameState(GameState.WIN);
    }

    // Coroutine này chỉ chịu trách nhiệm sinh ra quái của MỘT sóng
    private IEnumerator SpawnWaveCoroutine(Wave wave)
    {
        Debug.Log("Bắt đầu sinh quái cho sóng: " + wave.waveName);
        foreach (var group in wave.spawnGroups)
        {
            // Chạy Coroutine sinh ra từng nhóm quái song song
            StartCoroutine(SpawnGroupCoroutine(group));
        }
        yield break; // Kết thúc ngay, không cần chờ đợi gì cả
    }

    // Coroutine này sinh ra MỘT NHÓM QUÁI (giữ nguyên logic cũ)
    private IEnumerator SpawnGroupCoroutine(SpawnGroup group)
    {
        yield return new WaitForSeconds(group.spawnDelay);
        for (int i = 0; i < group.count; i++)
        {
            if (spawnPoints.Length == 0)
            {
                Debug.LogError("WaveSpawner chưa có Spawn Points nào được gán!");
                yield break;
            }
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
    public void OnAnEnemyWasKilled()
    {
        // Logic tính điểm hoặc kiểm tra nhiệm vụ phụ có thể được viết ở đây
        Debug.Log("Một kẻ địch đã bị tiêu diệt. Cộng điểm!");
    }
}