using UnityEngine;
using System.Collections.Generic;

public class LevelUpUIController : MonoBehaviour
{
    [Header("UI Chính")]
    public GameObject levelUpPanel; // Cái Panel to đùng (Cha của CardsContainer)

    [Header("Danh sách 3 Slot thẻ bài")]
    // Thay vì khai báo card1, card2... ta dùng mảng (List) cho chuyên nghiệp
    public UpgradeCardUI[] cardSlots;

    private void OnEnable()
    {
        PlayerStats.OnPlayerLevelUp += ShowLevelUpOptions;
    }

    private void OnDisable()
    {
        PlayerStats.OnPlayerLevelUp -= ShowLevelUpOptions;
    }

    void Start()
    {
        levelUpPanel.SetActive(false); // Tắt ngay từ đầu
    }

    // Hàm này để Test nhanh bằng phím L (như đã bàn)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) ShowLevelUpOptions();
    }

    private void ShowLevelUpOptions()
    {
        // 1. Tạm dừng game
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. Lấy data
        List<UpgradeData> fullPool = PlayerStats.Instance.profile.availableUpgrades;

        // Nếu không có data nâng cấp nào -> Tắt bảng, chơi tiếp
        if (fullPool == null || fullPool.Count == 0)
        {
            EndLevelUpSequence();
            return;
        }

        // 3. Random ra tối đa 3 cái (hoặc ít hơn nếu không đủ data)
        int amountToPick = Mathf.Min(cardSlots.Length, fullPool.Count);
        List<UpgradeData> randomPicks = GetRandomUpgrades(fullPool, amountToPick);

        // 4. Bật Panel lên trước
        levelUpPanel.SetActive(true);


        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (i < randomPicks.Count)
            {
               
                cardSlots[i].Setup(randomPicks[i], OnUpgradeSelected);
            }
            else
            {
                Debug.LogWarning("Không đủ thẻ nâng cấp để lấp đầy tất cả các slot!");
            }
        }
    }

    // Xử lý khi chọn xong
    private void OnUpgradeSelected(UpgradeData selectedUpgrade)
    {
        // Cộng chỉ số
        PlayerStats.Instance.ApplyUpgrade(selectedUpgrade);

        // Xong việc -> Tắt toàn bộ Panel -> Đi chơi tiếp
        EndLevelUpSequence();
    }

    private void EndLevelUpSequence()
    {
        levelUpPanel.SetActive(false); // <--- TẮT CÁI TO NHƯ BẠN MUỐN
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Hàm Random giữ nguyên
    private List<UpgradeData> GetRandomUpgrades(List<UpgradeData> sourceList, int count)
    {
        List<UpgradeData> tempList = new List<UpgradeData>(sourceList);
        List<UpgradeData> result = new List<UpgradeData>();
        for (int i = 0; i < count; i++)
        {
            int r = Random.Range(0, tempList.Count);
            result.Add(tempList[r]);
            tempList.RemoveAt(r);
        }
        return result;
    }
}