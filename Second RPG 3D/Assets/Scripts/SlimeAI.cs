// SlimeAI.cs
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class SlimeAI : BaseAIController
{
    // Các trạng thái mà Slime có thể có
    public IdleState IdleState { get; private set; }
    public PatrolState PatrolState { get; private set; }
    public AlertState AlertState { get; private set; }
    public ChaseState ChaseState { get; private set; }
    public AttackState AttackState { get; private set; }
    public DieState DieState { get; private set; }

    [Header("Slime Specifics")]
    public Transform[] patrolPoints;

    protected override void InitializeStates()
    {
        // Tạo ra các phiên bản trạng thái cho con Slime này
        IdleState = new IdleState(this, patrolPoints);
        PatrolState = new PatrolState(this, patrolPoints);
        AlertState = new AlertState(this);
        ChaseState = new ChaseState(this);
        AttackState = new AttackState(this);
        DieState = new DieState(this);
    }

    protected override void SetInitialState()
    {
        // Slime sẽ bắt đầu ở trạng thái tuần tra
        ChangeState(PatrolState);
    }

    protected override void OnDamaged()
    {
        // Khi Slime bị đánh, nó sẽ ngay lập tức đuổi theo người chơi (giống trạng thái FURY cũ)
        if (!IsDead()) // Chỉ đuổi theo nếu chưa chết
        {
            ChangeState(ChaseState);
        }
    }

    protected override void OnDeath()
    {
        base.OnDeath(); // Gọi hàm OnDeath của lớp cha để in log
        Debug.Log("Slime explodes with a splat!"); // Thêm hiệu ứng đặc biệt
        // Gọi logic chết
        ChangeState(DieState);
    }
}