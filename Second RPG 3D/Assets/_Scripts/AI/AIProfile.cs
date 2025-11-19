using UnityEngine;

// Enum này sẽ quyết định "bộ não" ban đầu của AI
public enum AIBehaviorType
{
    PatrolAndChase, // Hành vi cũ: Tuần tra tự do và rượt đuổi người chơi
    Siege           // Hành vi mới: Công thành, đi đến một mục tiêu cố định
}

[CreateAssetMenu(fileName = "New AI Profile", menuName = "AI/AI Profile")]
public class AIProfile : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("AI này thuộc phe nào? Dùng để phân biệt địch – ta – trung lập.")]
    public Faction myFaction;

    [Header("Behavior")]
    [Tooltip("Kiểu hành vi chính của AI: Patrol, Guard, Siege...")]
    public AIBehaviorType behaviorType = AIBehaviorType.Siege;

    [Header("Health & Damage")]
    [Tooltip("Lượng máu tối đa của AI.")]
    public float maxHp = 10f;

    [Tooltip("Sát thương gây ra cho mục tiêu mỗi lần tấn công.")]
    public float attackDamage = 5f;

    [Header("Movement & Range")]
    [Tooltip("Tốc độ di chuyển khi tuần tra.")]
    public float patrolSpeed = 2f;

    [Tooltip("Tốc độ di chuyển khi đuổi theo mục tiêu.")]
    public float chaseSpeed = 4f;

    [Tooltip("Khoảng cách AI có thể nhìn và phát hiện mục tiêu.")]
    public float sightRange = 10f;

    [Tooltip("Khoảng cách cần thiết để AI bắt đầu tấn công.")]
    public float attackRange = 2f;

    [Tooltip("Bán kính di chuyển khi AI đi tuần.")]
    public float patrolRadius = 15f;

    [Header("Timings")]
    [Tooltip("Thời gian chờ giữa hai lần tấn công.")]
    public float attackDelay = 1.5f;

    [Tooltip("Thời gian AI đứng yên trong trạng thái Idle.")]
    public float idleWaitTime = 3f;

    [Tooltip("Thời gian AI duy trì trạng thái Alert trước khi quay về bình thường.")]
    public float alertWaitTime = 2f;

    [Tooltip("Thời gian chơi animation chết trước khi biến mất hoặc return về pool.")]
    public float dieAnimationTime = 1.7f;

    [Header("Rewards")]
    [Tooltip("Kinh nghiệm (XP) mà người chơi nhận khi tiêu diệt AI này.")]
    public int xpReward = 10;

    [Tooltip("Tag của Object Pool tương ứng. PHẢI trùng với tag đã đăng ký trong Object Pooler.")]
    public string poolTag;

}