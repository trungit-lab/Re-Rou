// BaseAIController.cs
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
    public AIStats stats;
    protected float currentHp;
    public bool IsPlayerVisible { get; set; }
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

        // --- DEBUG LOG: KIỂM TRA STATS ---
        if (stats == null)
        {
            Debug.LogError(gameObject.name + ": AIStats is not assigned in the Inspector!");
            return;
        }

        currentHp = stats.maxHp;
        if (healthBarObject != null)
        {
            healthBarSlider = healthBarObject.GetComponent<Slider>();
            healthBarSlider.maxValue = stats.maxHp;
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
        CurrentState?.Execute();

        if (enableSeparation)
        {
            ApplySeparation();
        }

        if (healthBarObject != null && healthBarObject.activeInHierarchy)
        {
            healthBarObject.transform.forward = mainCamera.transform.forward;
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
        // Chỉ thực hiện nếu AI chưa chết
        if (IsDead())
        {
            // Thêm log để biết tại sao không nhận sát thương nữa
            Debug.Log(gameObject.name + " is already dead, ignoring further damage.");
            return;
        }

        currentHp -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage. New HP: " + currentHp, gameObject);

        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHp;
        }

        // Luôn kích hoạt animation bị đánh
        animator.SetTrigger("GetHit");

        // === SỬA LỖI Ở ĐÂY ===
        // Kiểm tra lại điều kiện chết ngay sau khi trừ máu
        if (currentHp <= 0)
        {
            // Đảm bảo máu không bị âm để tránh gọi OnDeath nhiều lần
            currentHp = 0;

            Debug.LogWarning(gameObject.name + " HP is now <= 0. Calling OnDeath().", gameObject);
            OnDeath();
        }
        else
        {
            // Chỉ gọi OnDamaged nếu chưa chết
            OnDamaged();
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
        // --- DEBUG LOG: XÁC NHẬN HÀM ONDEATH ĐƯỢC GỌI ---
        Debug.LogWarning(gameObject.name + ": OnDeath() has been called. Disabling components and starting DieSequence.", gameObject);

        StopAllCoroutines();
        agent.isStopped = true;
        agent.enabled = false;
        mainCollider.enabled = false;
        if (healthBarObject != null) healthBarObject.SetActive(false);
        StartCoroutine(DieSequence());
    }

    protected virtual IEnumerator DieSequence()
    {
        // --- DEBUG LOG: XÁC NHẬN COROUTINE CHẾT BẮT ĐẦU ---
        Debug.Log(gameObject.name + ": DieSequence Coroutine started. Playing 'Die' animation.", gameObject);

        animator.SetTrigger("Die");

        // --- DEBUG LOG: THÔNG BÁO THỜI GIAN CHỜ ---
        Debug.Log(gameObject.name + ": Waiting for " + stats.dieAnimationTime + " seconds before destroying.", gameObject);

        yield return new WaitForSeconds(stats.dieAnimationTime);

        // --- DEBUG LOG: XÁC NHẬN TRƯỚC KHI HỦY ---
        Debug.Log(gameObject.name + ": Wait time finished. Creating gem and destroying GameObject.", gameObject);

        gameManager.CreateGem(transform);
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
        if (stats == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stats.sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.attackRange);
        if (enableSeparation)
        {
            Gizmos.color = new Color(0, 1, 1, 0.25f);
            Gizmos.DrawWireSphere(transform.position, separationDistance);
        }
    }
    #endregion
}

// Giữ nguyên AIStats và IState Interface
[System.Serializable]
public class AIStats
{
    public float maxHp = 10f;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float sightRange = 10f;
    public float attackRange = 2f;
    public float attackDelay = 1.5f;
    public float idleWaitTime = 3f;
    public float alertWaitTime = 2f;
    public float dieAnimationTime = 1.7f;
    public float patrolRadius = 15f;
}

public interface IState
{
    void Enter();
    void Execute();
    void Exit();
}