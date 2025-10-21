// AttackData.cs (Phiên bản đã tối giản, không chứa thông tin VFX)
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Player/Attack Data")]
public class AttackData : ScriptableObject
{
    [Header("Core Info")]
    public string animationTrigger;

    [Header("Stats")]
    public float damageMultiplier = 1f;
    public float hitRange = 1.5f;
    public float attackCooldown = 0.7f;
    public LayerMask hitMask;

    [Tooltip("Lực đẩy tác dụng lên kẻ địch khi bị trúng đòn. Đặt là 0 nếu không có. Kẻ địch cần có Rigidbody để hoạt động.")]
    public float knockbackForce = 0f;


    [Header("Movement (Dành riêng cho kỹ năng Dash)")]
    public bool isDashSkill = false;
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
}