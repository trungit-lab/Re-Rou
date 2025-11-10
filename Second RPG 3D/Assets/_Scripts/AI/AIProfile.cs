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
    [Tooltip("AI này thuộc phe nào?")]
    public Faction myFaction; // THÊM MỚI
    [Header("Behavior")]
    public AIBehaviorType behaviorType = AIBehaviorType.Siege;

    [Header("Health & Damage")]
    public float maxHp = 10f;
    public float attackDamage = 5f;

    [Header("Movement & Range")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float sightRange = 10f;
    public float attackRange = 2f;
    public float patrolRadius = 15f;

    [Header("Timings")]
    public float attackDelay = 1.5f;
    public float idleWaitTime = 3f;
    public float alertWaitTime = 2f;
    public float dieAnimationTime = 1.7f;

    [Tooltip("Tag của Object Pool mà AI này thuộc về. PHẢI TRÙNG VỚI TAG TRONG OBJECT POOLER.")]
    public string poolTag;

}