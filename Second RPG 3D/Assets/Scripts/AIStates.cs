// AIStates.cs
using UnityEngine;
using UnityEngine.AI;

// --- IDLE STATE ---
public class IdleState : IState
{
    private readonly BaseAIController context;
    private readonly Transform[] patrolPoints;
    private float timer;


    public IdleState(BaseAIController controller, Transform[] points)
    {
        context = controller;
        patrolPoints = points;
    }

    public void Enter()
    {
        context.GetComponent<Animator>().SetBool("isWalk", false);
        timer = Time.time + context.stats.idleWaitTime;
    }

    public void Execute()
    {
        // Ưu tiên cao nhất: nếu thấy người chơi, chuyển sang Alert
        if (context.IsPlayerVisible)
        {
            (context as SlimeAI).ChangeState((context as SlimeAI).AlertState);
            return;
        }

        // Hết thời gian chờ, quyết định đi tuần tra
        if (Time.time >= timer)
        {
            (context as SlimeAI).ChangeState((context as SlimeAI).PatrolState);
        }
    }
    public void Exit() { }
}

// --- PATROL STATE ---
public class PatrolState : IState
{
    private readonly BaseAIController context;
    private readonly Transform[] waypoints; // Chúng ta vẫn giữ lại để có lựa chọn

    public PatrolState(BaseAIController controller, Transform[] patrolPoints)
    {
        context = controller;
        waypoints = patrolPoints;
    }

    public void Enter()
    {
        context.GetComponent<Animator>().SetBool("isWalk", true);
        context.GetComponent<NavMeshAgent>().speed = context.stats.patrolSpeed;

        // --- LOGIC MỚI: ĐI LANG THANG NGẪU NHIÊN ---
        Vector3 randomPoint;
        if (TryGetRandomNavMeshPoint(context.transform.position, context.stats.patrolRadius, out randomPoint))
        {
            context.GetComponent<NavMeshAgent>().SetDestination(randomPoint);
        }
        else
        {
            // Nếu không tìm được điểm nào, quay về trạng thái nghỉ
            (context as SlimeAI).ChangeState((context as SlimeAI).IdleState);
        }
    }

    public void Execute()
    {
        // Ưu tiên cao nhất: nếu thấy người chơi, chuyển sang Alert
        if (context.IsPlayerVisible)
        {
            (context as SlimeAI).ChangeState((context as SlimeAI).AlertState);
            return;
        }

        // Khi đã đến gần điểm đến, chuyển sang trạng thái nghỉ
        var agent = context.GetComponent<NavMeshAgent>();
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            (context as SlimeAI).ChangeState((context as SlimeAI).IdleState);
        }
    }

    public void Exit()
    {
        context.GetComponent<NavMeshAgent>().ResetPath();
    }

    // Hàm tìm một điểm ngẫu nhiên trên NavMesh
    private bool TryGetRandomNavMeshPoint(Vector3 origin, float radius, out Vector3 result)
    {
        // Tạo một hướng ngẫu nhiên
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += origin;

        NavMeshHit navMeshHit;
        // Tìm điểm gần nhất trên NavMesh trong bán kính
        if (NavMesh.SamplePosition(randomDirection, out navMeshHit, radius, NavMesh.AllAreas))
        {
            result = navMeshHit.position;
            return true;
        }

        // Không tìm thấy điểm hợp lệ
        result = origin;
        return false;
    }
}

// --- ALERT STATE ---
public class AlertState : IState
{
    private readonly BaseAIController context;
    private float timer;

    public AlertState(BaseAIController controller)
    {
        context = controller;
    }

    public void Enter()
    {
        context.GetComponent<NavMeshAgent>().isStopped = true;
        context.GetComponent<Animator>().SetBool("isAlert", true);
        timer = Time.time + context.stats.alertWaitTime;
    }

    public void Execute()
    {
        // Luôn xoay mặt về phía người chơi
        Vector3 direction = (context.player.position - context.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        context.transform.rotation = Quaternion.Slerp(context.transform.rotation, lookRotation, Time.deltaTime * 5f);

        // Hết thời gian chờ
        if (Time.time >= timer)
        {
            if (context.IsPlayerVisible)
            {
                (context as SlimeAI).ChangeState((context as SlimeAI).ChaseState);
            }
            else
            {
                (context as SlimeAI).ChangeState((context as SlimeAI).PatrolState);
            }
        }
    }

    public void Exit()
    {
        context.GetComponent<NavMeshAgent>().isStopped = false;
        context.GetComponent<Animator>().SetBool("isAlert", false);
    }
}

// --- CHASE STATE ---
public class ChaseState : IState
{
    private readonly BaseAIController context;

    public ChaseState(BaseAIController controller)
    {
        context = controller;
    }

    public void Enter()
    {
        context.GetComponent<Animator>().SetBool("isWalk", true);
        context.GetComponent<Animator>().SetBool("isAlert", true);
        context.GetComponent<NavMeshAgent>().speed = context.stats.chaseSpeed;
    }

    public void Execute()
    {
        if (!context.IsPlayerVisible)
        {
            (context as SlimeAI).ChangeState((context as SlimeAI).IdleState);
            return;
        }

        context.GetComponent<NavMeshAgent>().SetDestination(context.player.position);
        float distance = Vector3.Distance(context.transform.position, context.player.position);

        if (distance <= context.stats.attackRange)
        {
            (context as SlimeAI).ChangeState((context as SlimeAI).AttackState);
        }
    }

    public void Exit()
    {
        context.GetComponent<Animator>().SetBool("isAlert", false);
    }
}

// --- ATTACK STATE ---
public class AttackState : IState
{
    private readonly BaseAIController context;
    private float timer;

    public AttackState(BaseAIController controller)
    {
        context = controller;
    }

    public void Enter()
    {
        context.GetComponent<NavMeshAgent>().isStopped = true;
        timer = 0; // Tấn công ngay lập tức
    }

    public void Execute()
    {
        // Xoay mặt về phía người chơi
        Vector3 direction = (context.player.position - context.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        context.transform.rotation = Quaternion.Slerp(context.transform.rotation, lookRotation, Time.deltaTime * 5f);

        if (Time.time >= timer)
        {
            context.GetComponent<Animator>().SetTrigger("isAttack");
            timer = Time.time + context.stats.attackDelay;
        }

        float distance = Vector3.Distance(context.transform.position, context.player.position);
        if (distance > context.stats.attackRange + 0.5f) // Thêm một khoảng đệm nhỏ
        {
            (context as SlimeAI).ChangeState((context as SlimeAI).ChaseState);
        }
    }

    public void Exit()
    {
        context.GetComponent<NavMeshAgent>().isStopped = false;
    }
}

// --- DIE STATE ---
public class DieState : IState
{
    private readonly BaseAIController context;
    public DieState(BaseAIController controller)
    {
        context = controller;
    }
    public void Enter()
    {
        // Logic chết đã được xử lý trong OnDeath() của BaseAIController
    }
    public void Execute() { }
    public void Exit() { }
}