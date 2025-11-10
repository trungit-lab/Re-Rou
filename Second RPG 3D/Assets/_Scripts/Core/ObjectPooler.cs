// Thay thế toàn bộ file ObjectPooler.cs bằng phiên bản này
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;

    void Awake() { Instance = this; }

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag)) return null;

        if (poolDictionary[tag].Count == 0)
        {
            // Tùy chọn: Mở rộng pool nếu hết đối tượng
            Pool poolToExpand = pools.Find(p => p.tag == tag);
            if (poolToExpand != null)
            {
                GameObject obj = Instantiate(poolToExpand.prefab);
                obj.SetActive(false);
                poolDictionary[tag].Enqueue(obj);
            }
            else
            {
                return null; // Không thể mở rộng
            }
            Debug.LogWarning("Pool với tag " + tag + " đã hết đối tượng.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Cảnh báo: Cố gắng trả đối tượng về một kho không tồn tại với tag: " + tag);
            // Nếu không có kho, cách an toàn nhất là hủy luôn đối tượng để tránh rò rỉ bộ nhớ
            Destroy(objectToReturn);
            return;
        }

        // Tắt đối tượng đi
        objectToReturn.SetActive(false);

        // Bỏ nó trở lại vào cuối hàng đợi của kho
        poolDictionary[tag].Enqueue(objectToReturn);
    }
}