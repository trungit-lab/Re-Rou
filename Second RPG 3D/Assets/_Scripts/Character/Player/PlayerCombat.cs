// PlayerCombat.cs (Phiên bản đã tái cấu trúc, quản lý VFX hoàn toàn bên trong)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(PlayerStats), typeof(CharacterController))]
public class PlayerCombat : MonoBehaviour
{
    // *** THÊM MỚI 1: Định nghĩa Enum và Class để nhóm dữ liệu trong Inspector ***
    public enum VFXActivationType
    {
        InstantiateOnUse,
        ToggleGameObject
    }

    [System.Serializable] // Dòng này rất quan trọng để Unity hiển thị class này trong Inspector
    public class AttackEntry
    {
        [Tooltip("Chỉ để mô tả cho dễ nhận biết trong Inspector.")]
        public string description;
        public AttackData attackData; // Kéo file AttackData vào đây

        [Header("VFX Configuration for this Attack")]
        public VFXActivationType vfxType = VFXActivationType.InstantiateOnUse;
        [Tooltip("Dành cho Instantiate: Kéo Prefab VFX vào đây.")]
        public GameObject attackVFX;
        [Tooltip("Dành cho ToggleGameObject: Nhập chỉ số của VFX trong danh sách Player VFXs (bắt đầu từ 0).")]
        public int vfxIndex = -1;
    }

    // === Components & Dependencies ===
    private Animator amin;
    private PlayerStats playerStats;
    private CharacterController ccl;

    // *** SỬA ĐỔI: Danh sách `attacks` bây giờ là một danh sách các `AttackEntry` ***
    [Header("Attack Configuration")]
    public List<AttackEntry> attacks;

    [Header("Skill Config")]
    public AttackData dashSkillData; // Giữ nguyên Dash để không ảnh hưởng nhiều

    [Header("VFX Management")]
    [Tooltip("Kéo TẤT CẢ các GameObject VFX của Player vào danh sách này.")]
    public List<GameObject> playerVFXs;

    // === Trạng thái ===
    private bool isAttacking = false;
    private bool canDash = true;
    private int currentAttackIndex = -1;
    private float[] attackCooldownTimers;

    private void Awake()
    {
        amin = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
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

        if (isAttacking || playerStats.IsDead()) return;
        if (attackIndex < 0 || attackIndex >= attacks.Count) return;
        if (Time.time < attackCooldownTimers[attackIndex]) return;

        currentAttackIndex = attackIndex;
        // Lấy cooldown từ AttackData bên trong AttackEntry
        attackCooldownTimers[attackIndex] = Time.time + attacks[attackIndex].attackData.attackCooldown;
        StartCoroutine(AttackCoroutine(attacks[currentAttackIndex]));
    }

    public void Dash()
    {
        if (isAttacking || !canDash || playerStats.IsDead() || dashSkillData == null) return;
        currentAttackIndex = -1;
        StartCoroutine(DashCoroutine(dashSkillData));
    }


    // --- COROUTINES (Đã sửa để nhận AttackEntry) ---
    private IEnumerator AttackCoroutine(AttackEntry attackEntry)
    {
        isAttacking = true;
        amin.SetTrigger(attackEntry.attackData.animationTrigger);
        HandleVFX(attackEntry, true); // Truyền cả AttackEntry vào

        yield return new WaitForSeconds(attackEntry.attackData.attackCooldown * 0.8f);

        HandleVFX(attackEntry, false);
        isAttacking = false;
    }

    // Coroutine Dash vẫn giữ nguyên để đảm bảo không bị lỗi
    private IEnumerator DashCoroutine(AttackData dashData)
    {
        isAttacking = true;
        canDash = false;
        amin.SetTrigger(dashData.animationTrigger);
        // Logic HandleVFX cho Dash cần được xử lý riêng nếu Dash cũng cần hiệu ứng
        // (Hiện tại chưa có, bạn có thể thêm sau nếu muốn)

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


    // *** SỬA ĐỔI QUAN TRỌNG: Hàm HandleVFX nhận AttackEntry ***
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



    public void Hit()
    {

        if (currentAttackIndex < 0 || currentAttackIndex >= attacks.Count) return;

        AttackData currentAttackData = attacks[currentAttackIndex].attackData;
        if (currentAttackData.damageMultiplier <= 0) return;

        float finalDamage = playerStats.baseDamage * currentAttackData.damageMultiplier;

        Collider[] hitInfor = Physics.OverlapSphere(transform.position + transform.forward * 1.0f, currentAttackData.hitRange, currentAttackData.hitMask);

        foreach (Collider c in hitInfor)
        {
            // --- LOGIC KIỂM TRA PHE PHÁI (THÊM MỚI) ---

            // 1. Lấy thông tin Targetable của mục tiêu
            Targetable targetInfo = c.GetComponentInParent<Targetable>();

            // 2. Bỏ qua nếu đối tượng không thể bị nhắm tới (không có script Targetable)
            if (targetInfo == null)
            {
                continue; // Bỏ qua vật thể này và xét vật thể tiếp theo
            }

            // 3. Bỏ qua nếu mục tiêu là chính mình hoặc cùng phe Player
            //    (Giả sử Player luôn thuộc phe Faction.Player)
            if (targetInfo.gameObject == this.gameObject || targetInfo.faction == Faction.Player)
            {
                continue; // Bỏ qua đồng đội và xét vật thể tiếp theo
            }

            // --- NẾU VƯỢT QUA CÁC BƯỚC KIỂM TRA, MỚI GÂY SÁT THƯƠNG ---
            //Debug.Log($"<color=cyan>Player tấn công hợp lệ vào: {c.name}</color>");

            // --- Phần gây sát thương (giữ nguyên) ---
            c.gameObject.SendMessage("GetHit", finalDamage, SendMessageOptions.DontRequireReceiver);

            if (currentAttackData.knockbackForce > 0 || currentAttackData.knockupForce > 0)
            {
                Rigidbody enemyRigidbody = c.GetComponent<Rigidbody>();
                if (enemyRigidbody != null)
                {
                    // 1. Tính toán vector lực đẩy lùi (nằm ngang)
                    Vector3 knockbackVector = (c.transform.position - transform.position).normalized;
                    knockbackVector.y = 0;
                    knockbackVector *= currentAttackData.knockbackForce;

                    // 2. Tính toán vector lực hất tung (thẳng đứng)
                    Vector3 knockupVector = Vector3.up * currentAttackData.knockupForce;

                    // 3. Cộng hai vector lực lại và áp dụng một lần duy nhất
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