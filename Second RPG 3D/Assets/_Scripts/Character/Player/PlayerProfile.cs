// FILE: PlayerProfile.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Player Profile", menuName = "Player/Player Profile")]
public class PlayerProfile : ScriptableObject
{
    [Header("Base Stats")]
    [Tooltip("Máu tối đa khởi điểm.")]
    public float maxHp = 100f;

    [Tooltip("Sát thương cơ bản khởi điểm.")]
    public float baseDamage = 10f;

    // --- BẠN CÓ THỂ THÊM CÁC CHỈ SỐ GỐC KHÁC Ở ĐÂY ---
    // public float moveSpeed = 5f;
    // public float attackSpeed = 1f;

    [Header("Leveling System")]
    [Tooltip("Lượng XP cần thiết để từ cấp 1 lên cấp 2.")]
    public float requiredXpForLevel2 = 100f;

    [Tooltip("Hệ số nhân cho lượng XP yêu cầu ở cấp tiếp theo. Ví dụ: 1.2 = tăng 20%.")]
    public float requiredXpMultiplier = 1.2f;

    [Header("Upgrade Pool")]
    [Tooltip("Kéo TẤT CẢ các file UpgradeData có thể có của nhân vật này vào đây.")]
    public List<UpgradeData> availableUpgrades;
}