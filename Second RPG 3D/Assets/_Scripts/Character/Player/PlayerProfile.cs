// FILE: PlayerProfile.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Player Profile", menuName = "Player/Player Profile")]
public class PlayerProfile : ScriptableObject
{
    [Header("Base Combat Stats")]
    [Tooltip("Máu tối đa khởi điểm.")]
    public float maxHp = 100f;

    [Tooltip("Sát thương cơ bản khởi điểm cho mỗi đòn đánh.")]
    public float baseDamage = 10f;

    [Header("Base Movement Stats")]
    [Tooltip("Tốc độ di chuyển cơ bản.")]
    public float moveSpeed = 5f;

    // --- BẠN CÓ THỂ THÊM CÁC CHỈ SỐ GỐC KHÁC Ở ĐÂY ---
    // Ví dụ:
    // public float attackCooldownMultiplier = 1f; // 1 = 100% tốc độ, 0.8 = nhanh hơn 20%
    // public float defense = 0f;

    [Header("Leveling System")]
    [Tooltip("Lượng XP cần thiết để từ cấp 1 lên cấp 2.")]
    public float requiredXpForLevel2 = 100f;

    [Tooltip("Hệ số nhân cho lượng XP yêu cầu ở cấp tiếp theo. Ví dụ: 1.2 = tăng 20% mỗi cấp.")]
    public float requiredXpMultiplier = 1.2f;

    [Header("Upgrade Pool")]
    [Tooltip("Kéo TẤT CẢ các file UpgradeData có thể có của nhân vật này vào đây. " +
             "Hệ thống sẽ rút ngẫu nhiên từ danh sách này khi lên cấp.")]
    public List<UpgradeData> availableUpgrades;
}