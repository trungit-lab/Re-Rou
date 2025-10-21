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
        Player,
        PlayerUnit,
        DefensiveStructure,
        ObstacleStructure,
        MainObjective
    }

    [Tooltip("Phe của đối tượng này.")]
    public Faction faction;

    [Tooltip("Loại của đối tượng này (để xác định độ ưu tiên).")]
    public TargetType type;
}