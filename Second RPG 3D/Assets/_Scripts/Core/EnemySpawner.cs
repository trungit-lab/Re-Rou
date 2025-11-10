using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("Prefab của quái vật sẽ được tạo ra.")]
    public GameObject enemyPrefab;

    [Tooltip("Thời gian chờ giữa mỗi lần sinh quái.")]
    public float spawnInterval = 3f;

    [Tooltip("Tổng số lượng quái sẽ được sinh ra từ spawner này.")]
    public int totalEnemiesToSpawn = 10;


    [Tooltip("Mục tiêu chính cho những con quái được sinh ra từ spawner NÀY.")]
    public Transform mainObjectiveForSpawnedEnemies;
   

    [Header("Spawn Position")]
    [Tooltip("Điểm spawn cố định của quái vật (bắt buộc).")]
    public Transform spawnPoint;

    [Tooltip("Bán kính kiểm tra vật cản khi spawn.")]
    public float collisionCheckRadius = 0.5f;

    private int enemiesSpawned = 0;

    void Start()
    {
        if (Application.isPlaying)
        {
            if (spawnPoint == null)
            {
                Debug.LogError($" Spawner '{name}' chưa gán SpawnPoint!");
                return;
            }

        
            if (mainObjectiveForSpawnedEnemies == null)
            {
                Debug.LogWarning($"Spawner '{name}' chưa gán mục tiêu cho quái. Quái công thành có thể sẽ đứng yên.", this.gameObject);
            }


            StartCoroutine(SpawnEnemyRoutine());
        }
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        while (enemiesSpawned < totalEnemiesToSpawn)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (enemyPrefab == null)
            {
                Debug.LogError(" Chưa gán Enemy Prefab cho Spawner!", this.gameObject);
                yield break;
            }

            Vector3 spawnPos = spawnPoint.position;

            if (Physics.CheckSphere(spawnPos, collisionCheckRadius))
            {
                Debug.LogWarning($" Vị trí spawn {spawnPoint.name} bị cản!");
                continue;
            }

            // --- LOGIC SINH QUÁI ĐÃ ĐƯỢC THAY THẾ HOÀN TOÀN ---
            // 1. Tạo ra con quái và lưu nó vào một biến tạm
            GameObject newEnemyObject = Instantiate(enemyPrefab, spawnPos, spawnPoint.rotation);

            // 2. Lấy component AI ("bộ não") từ con quái vừa tạo
            BaseAIController aiController = newEnemyObject.GetComponentInChildren<BaseAIController>();

            // 3. Nếu con quái có bộ não VÀ spawner này có mục tiêu để gán...
            if (aiController != null && mainObjectiveForSpawnedEnemies != null)
            {
                // ...thì ra lệnh cho nó: "Mục tiêu của mày là đây!"
                aiController.mainObjectiveTarget = mainObjectiveForSpawnedEnemies;
            }
            // ----------------------------------------------------

            enemiesSpawned++;
        }

        Debug.Log($"Spawner '{name}' đã sinh đủ quái vật!");
    }

    private void OnDrawGizmos()
    {
        if (spawnPoint == null) return;

        // Spawner gốc
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // Điểm spawn
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(spawnPoint.position, 0.3f);

        // Đường nối
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, spawnPoint.position);

#if UNITY_EDITOR
        // Label hiển thị tên
        UnityEditor.Handles.Label(spawnPoint.position + Vector3.up * 0.5f, "Spawn Point");
#endif
    }
}