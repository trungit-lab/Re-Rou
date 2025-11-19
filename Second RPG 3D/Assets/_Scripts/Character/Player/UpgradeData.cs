// FILE: UpgradeData.cs
using UnityEngine;

// Định nghĩa các loại chỉ số có thể được nâng cấp
public enum StatType
{
    Health,
    Damage,
    AttackSpeed,
    MoveSpeed,
    // Thêm các chỉ số khác ở đây trong tương lai
}

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Player/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Tooltip("Loại chỉ số mà thẻ bài này sẽ nâng cấp.")]
    public StatType statType;

    [Tooltip("Tên của nâng cấp sẽ hiển thị trên thẻ bài.")]
    public string upgradeName;

    [TextArea(3, 5)]
    [Tooltip("Mô tả chi tiết về nâng cấp.")]
    public string upgradeDescription;

    [Tooltip("Giá trị cộng thêm vào chỉ số.")]
    public float value;

    // (Tùy chọn) Thêm Icon, màu sắc, độ hiếm... cho thẻ bài ở đây
    // public Sprite icon;
    // public Color cardColor;
}