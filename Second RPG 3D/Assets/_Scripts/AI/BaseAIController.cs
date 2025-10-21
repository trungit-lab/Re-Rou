// BaseAIController.cs (Phiên bản tích hợp Hệ Thống Mục Tiêu Động & Phe Phái)
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(Collider))]
public abstract class BaseAIController : MonoBehaviour
{
    #region Dependencies & Components
    [Header("Core Components")]
    public Animator animator;
    public NavMeshAgent agent;
    public Collider mainCollider;

    [Header("Core UI")]
    [SerializeField] protected GameObject healthBarObject;
    protected Slider healthBarSlider;

    [HideInInspector] public Transform player;
    [HideInInspector] public Camera mainCamera;
    [HideInInspector] public GameManager gameManager;
    #endregion

    #region State Machine
    public IState CurrentState { get; protected set; }
    #endregion

    #region Properties
    [Header("Core Stats")]
    public AIProfile profile;
    protected float currentHp;
    public bool IsPlayerVisible { get; set; } // Giữ lại để có thể dùng cho các logic đặc biệt
    #endregion

    #region Targeting System
    [Header("Dynamic Targeting")]
    [Tooltip("Mục tiêu chiến lược cuối cùng cho chế độ Công Thành (ví dụ: Cổng Dịch Chuyển).")]
    public Transform mainObjectiveTarget; // THÊM MỚI

    [Tooltip("Bán kính AI sẽ quét để tìm mục tiêu mới.")]
    public float targetScanRadius = 20f;
    [Tooltip("Layer chứa tất cả các đối tượng có thể bị tấn công (Player, Structures, Units).")]
    public LayerMask targetableLayer;


    // MỤC TIÊU HIỆN TẠI: Sẽ được cập nhật liên tục bởi hệ thống quét
    public Transform currentTarget { get; private set; }
    private float timeSinceLastScan = 0f;
    private const float SCAN_INTERVAL = 0.5f; // Quét tìm mục tiêu mới mỗi 0.5 giây
    #endregion

    #region Separation Behavior
    [Header("Separation Settings")]
    public bool enableSeparation = true;
    public float separationDistance = 5.0f;
    public float separationForce = 2.0f;
    public LayerMask separationLayer;

    private Collider[] nearbyAgents;
    private const int MAX_NEARBY_AGENTS = 10;
    #endregion

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        mainCollider = GetComponent<Collider>();
        InitializeStates();
    }

    protected virtual void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        player = gameManager.player;
        mainCamera = Camera.main;

        if (profile == null)
        {
            Debug.LogError(gameObject.name + ": AIProfile is not assigned in the Inspector!");
            return;
        }

        currentHp = profile.maxHp;
        if (healthBarObject != null)
        {
            healthBarSlider = healthBarObject.GetComponent<Slider>();
            healthBarSlider.maxValue = profile.maxHp;
            healthBarSlider.value = currentHp;
        }

        if (enableSeparation)
        {
            nearbyAgents = new Collider[MAX_NEARBY_AGENTS];
        }

        SetInitialState();
    }

    protected virtual void Update()
    {
        // Cập nhật trạng thái hiện tại
        CurrentState?.Execute();

        // Cập nhật mục tiêu định kỳ để tiết kiệm hiệu năng
        timeSinceLastScan += Time.deltaTime;
        if (timeSinceLastScan >= SCAN_INTERVAL)
        {
            UpdateTarget();
            timeSinceLastScan = 0f;
        }

        if (enableSeparation)
        {
            ApplySeparation();
        }

        if (healthBarObject != null && healthBarObject.activeInHierarchy)
        {
            healthBarObject.transform.forward = mainCamera.transform.forward;
        }
    }

    // THÊM MỚI: HÀM QUÉT VÀ ƯU TIÊN MỤC TIÊU
    protected virtual void UpdateTarget()
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, targetScanRadius, targetableLayer);

        Transform bestTarget = null;
        int highestPriority = -1;
        float closestDistanceSqr = Mathf.Infinity;

        if (profile.myFaction == Faction.Neutral) return; // AI phe Neutral không tấn công

        foreach (var targetCollider in targetsInViewRadius)
        {
            Targetable targetInfo = targetCollider.GetComponent<Targetable>();
            if (targetInfo == null) continue;

            // QUY TẮC PHE PHÁI: Bỏ qua mục tiêu cùng phe hoặc phe trung lập
            if (targetInfo.faction == profile.myFaction || targetInfo.faction == Faction.Neutral)
            {
                continue;
            }

            int currentPriority = GetPriority(targetInfo.type);

            // So sánh ưu tiên: Nếu mục tiêu mới có ưu tiên cao hơn, chọn nó
            if (currentPriority > highestPriority)
            {
                highestPriority = currentPriority;
                bestTarget = targetInfo.transform;
                closestDistanceSqr = (transform.position - bestTarget.position).sqrMagnitude;
            }
            // Nếu ưu tiên bằng nhau, chọn mục tiêu nào gần hơn
            else if (currentPriority == highestPriority)
            {
                float distanceSqr = (transform.position - targetInfo.transform.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    bestTarget = targetInfo.transform;
                    closestDistanceSqr = distanceSqr;
                }
            }
        }
        currentTarget = bestTarget;
    }

    // THÊM MỚI: HÀM ĐỊNH NGHĨA ĐỘ ƯU TIÊN
    private int GetPriority(Targetable.TargetType type)
    {
        switch (type)
        {
            case Targetable.TargetType.Player: return 5;
            case Targetable.TargetType.PlayerUnit: return 4;
            case Targetable.TargetType.DefensiveStructure: return 3;
            case Targetable.TargetType.ObstacleStructure: return 2;
            case Targetable.TargetType.MainObjective: return 1;
            default: return 0;
        }
    }

    public void ChangeState(IState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public virtual void GetHit(float damage)
    {
        if (IsDead()) return;
        currentHp -= damage;
        if (healthBarSlider != null) healthBarSlider.value = currentHp;
        animator.SetTrigger("GetHit");
        if (currentHp <= 0)
        {
            currentHp = 0;
            OnDeath();
        }
        else
        {
            OnDamaged();
        }
    }

    // CẬP NHẬT: Hàm này giờ sẽ tấn công currentTarget thay vì chỉ player
    public void AttackHit()
    {
        if (IsDead() || currentTarget == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        if (distanceToTarget <= profile.attackRange + 0.5f) // Thêm khoảng đệm nhỏ
        {
            // Cần một hệ thống nhận sát thương chung cho các mục tiêu
            // Ví dụ: IDamageable damageable = currentTarget.GetComponent<IDamageable>();
            // if(damageable != null) { damageable.TakeDamage(profile.attackDamage); }

            // Tạm thời, ta có thể thử lấy các loại component khác nhau
            PlayerStats playerStats = currentTarget.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.GetHit(profile.attackDamage);
            }
            // Thêm logic gây sát thương cho các loại mục tiêu khác ở đây
        }
    }

    protected virtual void ApplySeparation()
    {
        if (agent == null || !agent.enabled || IsDead()) return;
        Vector3 separationVector = Vector3.zero;
        int count = 0;
        int numFound = Physics.OverlapSphereNonAlloc(transform.position, separationDistance, nearbyAgents, separationLayer);

        for (int i = 0; i < numFound; i++)
        {
            Collider otherCollider = nearbyAgents[i];
            if (otherCollider.gameObject == gameObject) continue;
            Vector3 awayFromAgent = transform.position - otherCollider.transform.position;
            separationVector += awayFromAgent.normalized / awayFromAgent.magnitude;
            count++;
        }

        if (count > 0)
        {
            separationVector /= count;
            agent.Move(separationVector * separationForce * Time.deltaTime);
        }
    }

    #region Abstract & Virtual Methods
    protected abstract void InitializeStates();
    protected abstract void SetInitialState();
    protected virtual void OnDamaged() { }
    protected virtual void OnDeath()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDefeated(this);
        }

        StopAllCoroutines();
        agent.isStopped = true;
        agent.enabled = false;
        mainCollider.enabled = false;
        if (healthBarObject != null) healthBarObject.SetActive(false);
        StartCoroutine(DieSequence());
    }

    protected virtual IEnumerator DieSequence()
    {
        animator.SetTrigger("Die");
        yield return new WaitForSeconds(profile.dieAnimationTime);
        Destroy(gameObject);
    }
    #endregion

    #region Helper Methods
    public bool IsDead()
    {
        return currentHp <= 0;
    }
    #endregion

    #region Trigger Detection
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsPlayerVisible = true;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsPlayerVisible = false;
        }
    }
    #endregion

    #region Gizmos
    protected virtual void OnDrawGizmosSelected()
    {
        if (profile == null) return;
        // Gizmo cho bán kính quét mục tiêu
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, targetScanRadius);

        // Gizmo cho bán kính "nhìn" cũ (có thể vẫn hữu ích)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, profile.sightRange);

        // Gizmo cho tầm tấn công
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, profile.attackRange);

        if (enableSeparation)
        {
            Gizmos.color = new Color(0, 1, 1, 0.25f);
            Gizmos.DrawWireSphere(transform.position, separationDistance);
        }
    }
    #endregion
}

// Giữ nguyên IState Interface
public interface IState
{
    void Enter();
    void Execute();
    void Exit();
}