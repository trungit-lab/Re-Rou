using UnityEngine;

public class SlimeAI : BaseAIController
{
    // Các trạng thái mà Slime có thể có
    public IdleState IdleState { get; private set; }
    public PatrolState PatrolState { get; private set; }
    //public AlertState AlertState { get; private set; }
    public ChaseState ChaseState { get; private set; }
    public AttackState AttackState { get; private set; }
    public DieState DieState { get; private set; }
    // THÊM MỚI: Trạng thái công thành
    public SiegeState SiegeState { get; private set; }


    [Header("Slime Specifics")]
    [Tooltip("Các điểm để đi tuần tra, chỉ sử dụng khi AI có hành vi PatrolAndChase.")]
    public Transform[] patrolPoints;

    protected override void InitializeStates()
    {
        // Tạo ra các phiên bản trạng thái cho con Slime này
        IdleState = new IdleState(this, patrolPoints);
        PatrolState = new PatrolState(this, patrolPoints);
        //AlertState = new AlertState(this);
        ChaseState = new ChaseState(this);
        AttackState = new AttackState(this);
        DieState = new DieState(this);
        // THÊM MỚI: Khởi tạo trạng thái công thành
        SiegeState = new SiegeState(this);
    }

    protected override void SetInitialState()
    {
        // --- ĐÂY LÀ LOGIC RẼ NHÁNH QUAN TRỌNG NHẤT ---
        // Kiểm tra xem đã gán Profile trong Inspector chưa
        if (profile == null)
        {
            Debug.LogError("AI Profile chưa được gán cho " + gameObject.name + "! AI sẽ không hoạt động.", gameObject);
            return;
        }

        // Dựa vào loại hành vi được chọn trong AI Profile, quyết định trạng thái ban đầu
        switch (profile.behaviorType)
        {
            case AIBehaviorType.PatrolAndChase:
                // Nếu là quái đi tuần bình thường, bắt đầu bằng trạng thái nghỉ
                ChangeState(IdleState);
                break;

            case AIBehaviorType.Siege:
                // Nếu là quái công thành, bắt đầu ngay bằng trạng thái công thành
                ChangeState(SiegeState);
                break;

            default:
                // Mặc định, nếu có lỗi gì đó, cho nó đứng nghỉ
                ChangeState(IdleState);
                break;
        }
    }

    protected override void OnDamaged()
    {
        // Khi Slime bị đánh, nó sẽ ngay lập tức nổi giận và đuổi theo người chơi
        // Logic này hoạt động tốt cho cả hai chế độ:
        // - Quái tuần tra sẽ bỏ tuần tra và đuổi theo.
        // - Quái công thành sẽ tạm bỏ mục tiêu chính để tấn công người chơi cản đường.
        if (!IsDead())
        {
            ChangeState(ChaseState);
        }
    }

    protected override void OnDeath()
    {
        // Gọi hàm OnDeath() của lớp cha để xử lý các logic chung (tắt agent, collider,...)
        base.OnDeath();

        // Bạn có thể thêm các hiệu ứng đặc trưng cho Slime khi chết ở đây
        // Ví dụ: Debug.Log("Slime explodes with a splat!"); 

        // Chuyển sang trạng thái chết (dù trạng thái này không làm gì, nó giúp hệ thống logic được rõ ràng)
        ChangeState(DieState);
    }
}