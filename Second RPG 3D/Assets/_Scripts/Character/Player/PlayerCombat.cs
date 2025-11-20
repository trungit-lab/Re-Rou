// FILE: PlayerCombat.cs (Phiên bản nâng cấp cho Giai đoạn 2)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(CharacterController))]
public class PlayerCombat : MonoBehaviour
{
    public enum VFXActivationType
    {
        InstantiateOnUse,
        ToggleGameObject
    }

    [System.Serializable]
    public class AttackEntry
    {
        [Tooltip("Chỉ để mô tả cho dễ nhận biết trong Inspector.")]
        public string description;
        public AttackData attackData;

        [Header("VFX Configuration")]
        public VFXActivationType vfxType = VFXActivationType.InstantiateOnUse;
        public GameObject attackVFX;
        public int vfxIndex = -1;
    }

    // === Components & Dependencies ===
    private Animator amin;
    private CharacterController ccl;
    // Đã xóa tham chiếu PlayerStats vì dùng Singleton

    [Header("Attack Configuration")]
    public List<AttackEntry> attacks;

    [Header("Skill Config")]
    public AttackData dashSkillData;

    [Header("VFX Management")]
    public List<GameObject> playerVFXs;

    // === Trạng thái ===
    private bool isAttacking = false;
    private bool canDash = true;
    private int currentAttackIndex = -1;
    private float[] attackCooldownTimers;

    private void Awake()
    {
        amin = GetComponent<Animator>();
        ccl = GetComponent<CharacterController>();

        foreach (GameObject vfx in playerVFXs)
        {
            if (vfx != null) vfx.SetActive(false);
        }
    }

    private void Start()
    {
        if (attacks != null)
        {
            attackCooldownTimers = new float[attacks.Count];
        }
    }

    // --- CÁC HÀM PUBLIC ---
    public void Attack(int attackID)
    {
        int attackIndex = attackID - 1;

        // Sử dụng PlayerStats.Instance.IsDead() thay vì biến cục bộ
        if (isAttacking || (PlayerStats.Instance != null && PlayerStats.Instance.IsDead())) return;

        if (attackIndex < 0 || attackIndex >= attacks.Count) return;
        if (Time.time < attackCooldownTimers[attackIndex]) return;

        currentAttackIndex = attackIndex;
        attackCooldownTimers[attackIndex] = Time.time + attacks[attackIndex].attackData.attackCooldown;
        StartCoroutine(AttackCoroutine(attacks[currentAttackIndex]));
    }

    public void Dash()
    {
        if (isAttacking || !canDash || (PlayerStats.Instance != null && PlayerStats.Instance.IsDead()) || dashSkillData == null) return;
        currentAttackIndex = -1;
        StartCoroutine(DashCoroutine(dashSkillData));
    }

    // --- COROUTINES ---
    private IEnumerator AttackCoroutine(AttackEntry attackEntry)
    {
        isAttacking = true;
        amin.SetTrigger(attackEntry.attackData.animationTrigger);
        HandleVFX(attackEntry, true);

        yield return new WaitForSeconds(attackEntry.attackData.attackCooldown * 0.8f);

        HandleVFX(attackEntry, false);
        isAttacking = false;
    }

    private IEnumerator DashCoroutine(AttackData dashData)
    {
        isAttacking = true;
        canDash = false;
        amin.SetTrigger(dashData.animationTrigger);

        float startTime = Time.time;
        Vector3 dashDirection = transform.forward;
        while (Time.time < startTime + dashData.dashDuration)
        {
            ccl.Move(dashDirection * dashData.dashSpeed * Time.deltaTime);
            yield return null;
        }
        isAttacking = false;

        yield return new WaitForSeconds(dashData.attackCooldown);
        canDash = true;
    }

    private void HandleVFX(AttackEntry entry, bool activate)
    {
        if (entry.vfxType == VFXActivationType.InstantiateOnUse)
        {
            if (activate && entry.attackVFX != null)
            {
                Instantiate(entry.attackVFX, transform.position, transform.rotation);
            }
        }
        else if (entry.vfxType == VFXActivationType.ToggleGameObject)
        {
            if (entry.vfxIndex >= 0 && entry.vfxIndex < playerVFXs.Count)
            {
                GameObject vfxToToggle = playerVFXs[entry.vfxIndex];
                if (vfxToToggle != null)
                {
                    vfxToToggle.SetActive(activate);
                }
            }
        }
    }

    // --- HÀM GÂY SÁT THƯƠNG (Animation Event) ---
    public void Hit()
    {
        if (currentAttackIndex < 0 || currentAttackIndex >= attacks.Count) return;

        AttackData currentAttackData = attacks[currentAttackIndex].attackData;
        if (currentAttackData.damageMultiplier <= 0) return;

        // --- THAY ĐỔI QUAN TRỌNG: Lấy BaseDamage từ Singleton ---
        float baseDamage = (PlayerStats.Instance != null) ? PlayerStats.Instance.BaseDamage : 10f;
        float finalDamage = baseDamage * currentAttackData.damageMultiplier;

        Collider[] hitInfor = Physics.OverlapSphere(transform.position + transform.forward * 1.0f, currentAttackData.hitRange, currentAttackData.hitMask);

        foreach (Collider c in hitInfor)
        {
            // --- Logic kiểm tra Phe Phái (Giữ nguyên) ---
            Targetable targetInfo = c.GetComponentInParent<Targetable>();
            if (targetInfo == null) continue;
            if (targetInfo.gameObject == this.gameObject || targetInfo.faction == Faction.Player) continue;

            // --- Gây sát thương ---
            c.gameObject.SendMessage("GetHit", finalDamage, SendMessageOptions.DontRequireReceiver);

            // --- Hiệu ứng Hit Stop ---
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerHitStop(0.08f);
            }

            // --- Hiệu ứng rung màn hình (Tùy chọn thêm vào đây nếu muốn đòn đánh có lực hơn) ---
            // if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.1f, 0.1f);

            // --- Logic đẩy lùi / hất tung ---
            if (currentAttackData.knockbackForce > 0 || currentAttackData.knockupForce > 0)
            {
                Rigidbody enemyRigidbody = c.GetComponent<Rigidbody>();
                if (enemyRigidbody != null)
                {
                    Vector3 knockbackVector = (c.transform.position - transform.position).normalized;
                    knockbackVector.y = 0;
                    knockbackVector *= currentAttackData.knockbackForce;

                    Vector3 knockupVector = Vector3.up * currentAttackData.knockupForce;

                    enemyRigidbody.AddForce(knockbackVector + knockupVector, ForceMode.Impulse);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attacks == null || attacks.Count == 0) return;
        Gizmos.color = Color.red;
        if (attacks[0] != null && attacks[0].attackData != null)
            Gizmos.DrawWireSphere(transform.position + transform.forward * 1.0f, attacks[0].attackData.hitRange);
    }
}