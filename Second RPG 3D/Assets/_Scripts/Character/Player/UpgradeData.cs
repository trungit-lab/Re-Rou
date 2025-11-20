// FILE: UpgradeData.cs
using UnityEngine;

// Định nghĩa các loại chỉ số có thể được nâng cấp.
// Rất quan trọng: Tên trong đây phải được xử lý trong hàm ApplyUpgrade của PlayerStats.
public enum StatType
{
    Health,
    Damage,
    MoveSpeed
    // Thêm các chỉ số khác ở đây trong tương lai (ví dụ: AttackSpeed, CritChance...)
}

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Player/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Tooltip("Loại chỉ số mà thẻ bài này sẽ nâng cấp.")]
    public StatType statType;

    [Tooltip("Tên của nâng cấp sẽ hiển thị trên thẻ bài (ví dụ: 'Sức Mạnh Cường Hóa').")]
    public string upgradeName;

    [TextArea(3, 5)]
    [Tooltip("Mô tả chi tiết về nâng cấp (ví dụ: 'Tăng 5 Sát thương cơ bản').")]
    public string upgradeDescription;

    [Tooltip("Giá trị sẽ được cộng thêm vào chỉ số tương ứng.")]
    public float value;

    [Header("Visuals")]
    public Sprite icon;

    // (Tùy chọn) Màu nền thẻ bài để phân biệt độ hiếm (Trắng, Xanh, Vàng...) chức năng sau này
    // public Color themeColor = Color.white;  
}