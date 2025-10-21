// AIStates.cs (Phiên bản cập nhật để hoạt động với Hệ Thống Mục Tiêu Động)
using UnityEngine;
using UnityEngine.AI;

// --- IDLE STATE ---
public class IdleState : IState
{
    private readonly BaseAIController context;
    public IdleState(BaseAIController controller, Transform[] points) { context = controller; }

    public void Enter()
    {
        context.GetComponent<Animator>().SetBool("isWalk", false);
    }

    public void Execute()
    {
        // CẬP NHẬT: Nếu hệ thống quét tìm thấy một mục tiêu, chuyển sang tấn công/đuổi theo
        if (context.currentTarget != null)
        {
            // Dựa vào chế độ trong Profile để quyết định trạng thái tiếp theo
            if (context.profile.behaviorType == AIBehaviorType.Siege)
                (context as SlimeAI).ChangeState((context as SlimeAI).SiegeState);
            else
                (context as SlimeAI).ChangeState((context as SlimeAI).ChaseState);
            return;
        }

        // Nếu không có mục tiêu và là AI đi tuần, chuyển sang Patrol sau một lúc
        if (context.profile.behaviorType == AIBehaviorType.PatrolAndChase)
        {
            // Có thể thêm một timer ở đây để nó không chuyển state ngay lập tức
            (context as SlimeAI).ChangeState((context as SlimeAI).PatrolState);
        }
    }
    public void Exit() { }
}

// --- PATROL STATE ---
public class PatrolState : IState
{
    // ... (Giữ nguyên logic đi lang thang ngẫu nhiên, chỉ sửa Execute)
    private readonly BaseAIController context;
    public PatrolState(BaseAIController controller, Transform[] patrolPoints) { context = controller; }
    public void Enter()
    {
        context.GetComponent<Animator>().SetBool("isWalk", true);
        context.GetComponent<NavMeshAgent>().speed = context.profile.patrolSpeed;
        Vector3 randomPoint;
        if (TryGetRandomNavMeshPoint(context.transform.position, context.profile.patrolRadius, out randomPoint))
        {
            context.GetComponent<NavMeshAgent>().SetDestination(randomPoint);
        }
        else
        {
            (context as SlimeAI).ChangeState((context as SlimeAI).IdleState);
        }
    }
    public void Execute()
    {
        // Ưu tiên cao nhất: nếu tìm thấy mục tiêu, ngừng tuần tra và tấn công
        if (context.currentTarget != null)
        {
            (context as SlimeAI).ChangeState((context as SlimeAI).ChaseState);
            return;
        }
        var agent = context.GetComponent<NavMeshAgent>();
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            (context as SlimeAI).ChangeState((context as SlimeAI).IdleState);
        }
    }
    public void Exit() { if (context.IsDead()) return; context.GetComponent<NavMeshAgent>().ResetPath(); }
    private bool TryGetRandomNavMeshPoint(Vector3 origin, float radius, out Vector3 result) { Vector3 randomDirection = Random.insideUnitSphere * radius; randomDirection += origin; NavMeshHit navMeshHit; if (NavMesh.SamplePosition(randomDirection, out navMeshHit, radius, NavMesh.AllAreas)) { result = navMeshHit.position; return true; } result = origin; return false; }
}


// --- CHASE STATE --- (Dành cho AI tự do)
public class ChaseState : IState
{
    private readonly BaseAIController context;
    public ChaseState(BaseAIController controller) { context = controller; }

    public void Enter()
    {
        context.GetComponent<NavMeshAgent>().isStopped = false;
        context.GetComponent<Animator>().SetBool("isWalk", true);
        context.GetComponent<NavMeshAgent>().speed = context.profile.chaseSpeed;
    }

    public void Execute()
    {
        // Nếu mục tiêu đã bị tiêu diệt hoặc ngoài tầm quét, quay về nghỉ
        if (context.currentTarget == null)
        {
            (context as SlimeAI).ChangeState((context as SlimeAI).IdleState);
            return;
        }

        context.GetComponent<NavMeshAgent>().SetDestination(context.currentTarget.position);
        float distance = Vector3.Distance(context.transform.position, context.currentTarget.position);

        if (distance <= context.profile.attackRange)
        {
            (context as SlimeAI).ChangeState((context as SlimeAI).AttackState);
        }
    }
    public void Exit() { }
}

// --- ATTACK STATE --- (Trạng thái tấn công chung)
public class AttackState : IState
{
    private readonly BaseAIController context;
    private float timer;
    public AttackState(BaseAIController controller) { context = controller; }

    public void Enter()
    {
        context.GetComponent<NavMeshAgent>().isStopped = true;
        timer = 0; // Tấn công ngay
    }

    public void Execute()
    {
        if (context.currentTarget == null)
        {
            // Nếu mất mục tiêu, quay về trạng thái hành vi chính
            if (context.profile.behaviorType == AIBehaviorType.Siege)
                (context as SlimeAI).ChangeState((context as SlimeAI).SiegeState);
            else
                (context as SlimeAI).ChangeState((context as SlimeAI).IdleState);
            return;
        }

        Vector3 direction = (context.currentTarget.position - context.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        context.transform.rotation = Quaternion.Slerp(context.transform.rotation, lookRotation, Time.deltaTime * 5f);

        if (Time.time >= timer)
        {
            context.GetComponent<Animator>().SetTrigger("isAttack");
            timer = Time.time + context.profile.attackDelay;
        }

        float distance = Vector3.Distance(context.transform.position, context.currentTarget.position);
        if (distance > context.profile.attackRange + 0.5f)
        {
            // Mục tiêu chạy ra ngoài tầm, quay lại đuổi theo
            (context as SlimeAI).ChangeState((context as SlimeAI).ChaseState);
        }
    }

    public void Exit()
    {
        if (context.IsDead()) return;
        context.GetComponent<NavMeshAgent>().isStopped = false;
    }
}


// --- SIEGE STATE --- (Trạng thái di chuyển công thành)
public class SiegeState : IState
{
    private readonly BaseAIController context;
    public SiegeState(BaseAIController controller) { context = controller; }

    public void Enter()
    {
        context.GetComponent<NavMeshAgent>().isStopped = false;
        context.GetComponent<Animator>().SetBool("isWalk", true);
        context.GetComponent<NavMeshAgent>().speed = context.profile.chaseSpeed;
    }

    public void Execute()
    {
        if (context.currentTarget != null)
        {
            context.GetComponent<NavMeshAgent>().SetDestination(context.currentTarget.position);

            float distance = Vector3.Distance(context.transform.position, context.currentTarget.position);
            if (distance <= context.profile.attackRange)
            {
                (context as SlimeAI).ChangeState((context as SlimeAI).AttackState);
            }
        }
        else
        {
            // Nếu không có mục tiêu nào trong tầm quét, AI công thành có thể đứng chờ hoặc đi loanh quanh
            // Quay về Idle là một lựa chọn an toàn
            (context as SlimeAI).ChangeState((context as SlimeAI).IdleState);
        }
    }
    public void Exit() { }
}

// --- DIE STATE --- (Không thay đổi)
public class DieState : IState
{
    private readonly BaseAIController context;
    public DieState(BaseAIController controller) { context = controller; }
    public void Enter() { }
    public void Execute() { }
    public void Exit() { }
}

// AlertState có thể không còn cần thiết nữa, hoặc có thể được tái sử dụng
// như một trạng thái "gầm gừ" trước khi tấn công. Tạm thời chúng ta có thể không dùng đến nó.