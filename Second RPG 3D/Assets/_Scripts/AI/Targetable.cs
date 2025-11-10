using UnityEngine;

// Enum này có thể được đặt ở đây hoặc trong một file riêng
public enum Faction
{
    Neutral,
    Player,
    Enemy
}

public class Targetable : MonoBehaviour
{
    public enum TargetType
    {
        // --- CÁC LOẠI CŨ THUỘC PHE PLAYER (GIỮ NGUYÊN) ---
        Player,               // Nhân vật người chơi điều khiển
        PlayerUnit,           // Lính do người chơi triệu hồi
        DefensiveStructure,   // Trụ, công trình phòng thủ của Player
        ObstacleStructure,    // Tường, vật cản của Player
        MainObjective,        // Mục tiêu chính của Player (ví dụ: Nhà chính của địch)
                              // Hoặc cũng có thể là mục tiêu chính của địch (Cổng của Player)
                              // Chúng ta sẽ phân biệt bằng Faction.

        // --- CÁC LOẠI MỚI THÊM VÀO CHO PHE ENEMY ---
        Enemy_Normal,         // Quái Thường
        Enemy_Elite,          // Quái Tinh Anh
        Enemy_Boss,           // Quái Trùm
        EnemyPortal           // Cổng của Quái Vật (Mục tiêu của Người Chơi)
    }

    [Tooltip("Phe của đối tượng này.")]
    public Faction faction;

    [Tooltip("Loại của đối tượng này (để xác định độ ưu tiên).")]
    public TargetType type;
}