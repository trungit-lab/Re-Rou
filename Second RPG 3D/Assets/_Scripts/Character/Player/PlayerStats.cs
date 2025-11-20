// FILE: PlayerStats.cs (Phiên bản hoàn thiện cho Giai đoạn 2)
using UnityEngine;
using UnityEngine.UI;
using System; // Cần để sử dụng Action (Event)

public class PlayerStats : MonoBehaviour
{
    // --- SINGLETON: Để các script khác (Combat, UI) dễ dàng truy cập ---
    public static PlayerStats Instance { get; private set; }

    [Header("Data Source")]
    [Tooltip("BẮT BUỘC: Kéo file PlayerProfile vào đây.")]
    public PlayerProfile profile;

    [Header("UI References")]
    public Slider healthBar;
    [Tooltip("Thanh hiển thị kinh nghiệm.")]
    public Slider xpBar;
    [Tooltip("Text hiển thị cấp độ hiện tại.")]
    public TMPro.TMP_Text levelText;

    // --- RUNTIME STATS (Các chỉ số thực tế trong trận đấu) ---
    // Chúng ta dùng Property { get; private set; } để các script khác có thể ĐỌC nhưng không thể GHI đè tùy tiện.
    public float BaseDamage { get; private set; }
    public float MaxHp { get; private set; }
    public float CurrentHp { get; private set; }
    public float MoveSpeed { get; private set; }

    // --- LEVEL SYSTEM ---
    public int Level { get; private set; }
    public float CurrentXp { get; private set; }
    public float RequiredXp { get; private set; }

    // --- SỰ KIỆN ---
    // Báo cho hệ thống UI biết khi người chơi lên cấp để hiện bảng chọn thẻ bài
    public static event Action OnPlayerLevelUp;

    // --- COMPONENTS ---
    private Animator amin;
    private PlayerMovement playerMovement;
    private bool isDead = false;

    private void Awake()
    {
        // Khởi tạo Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        amin = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        if (profile == null)
        {
            Debug.LogError("LỖI NGHIÊM TRỌNG: Chưa gán PlayerProfile cho PlayerStats!", gameObject);
            return;
        }

        InitializeStatsFromProfile();
    }

    // --- KHỞI TẠO CHỈ SỐ TỪ PROFILE ---
    private void InitializeStatsFromProfile()
    {
        // 1. Khởi tạo Level
        Level = 1;
        CurrentXp = 0;
        RequiredXp = profile.requiredXpForLevel2;

        // 2. Khởi tạo Combat Stats
        MaxHp = profile.maxHp;
        CurrentHp = MaxHp;
        BaseDamage = profile.baseDamage;

        // 3. Khởi tạo Movement Stats
        MoveSpeed = profile.moveSpeed;
        if (playerMovement != null)
        {
            playerMovement.SetMovementSpeed(MoveSpeed);
        }

        // 4. Cập nhật UI lần đầu
        UpdateAllUI();
    }

    // --- HỆ THỐNG LEVEL UP ---
    public void GainXp(int amount)
    {
        if (isDead) return;

        CurrentXp += amount;

        // Kiểm tra xem có đủ XP để lên cấp không
        while (CurrentXp >= RequiredXp)
        {
            PerformLevelUp();
        }

        UpdateLevelUI();
    }

    private void PerformLevelUp()
    {
        CurrentXp -= RequiredXp;
        Level++;

        // Tính XP cần cho cấp tiếp theo dựa trên hệ số nhân trong Profile
        RequiredXp *= profile.requiredXpMultiplier;

        Debug.Log($"<color=yellow>LÊN CẤP! Đạt cấp {Level}</color>");

        // Hồi đầy máu khi lên cấp (Phần thưởng nhỏ)
        HealToFull();

        // Phát sự kiện để UI Controller hiện bảng chọn thẻ bài
        OnPlayerLevelUp?.Invoke();
    }

    // --- HỆ THỐNG NÂNG CẤP (Được gọi từ UI Thẻ Bài) ---
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null) return;

        switch (upgrade.statType)
        {
            case StatType.Health:
                MaxHp += upgrade.value;
                CurrentHp += upgrade.value; // Tăng máu tối đa thì hồi luôn lượng đó
                Debug.Log($"Nâng cấp: Máu tối đa +{upgrade.value}");
                break;

            case StatType.Damage:
                BaseDamage += upgrade.value;
                Debug.Log($"Nâng cấp: Sát thương +{upgrade.value}");
                break;

            case StatType.MoveSpeed:
                MoveSpeed += upgrade.value;
                if (playerMovement != null)
                {
                    playerMovement.SetMovementSpeed(MoveSpeed);
                }
                Debug.Log($"Nâng cấp: Tốc độ +{upgrade.value}");
                break;
        }

        UpdateAllUI();
    }

    // --- HỆ THỐNG COMBAT (Nhận Sát Thương & Hồi Máu) ---
    public void GetHit(float amount)
    {
        if (isDead) return;

        CurrentHp -= amount;
        UpdateHealthUI();

        // Kích hoạt hiệu ứng rung màn hình (nếu đã làm ở Giai đoạn 1)
       

        if (CurrentHp > 0)
        {
            amin.SetTrigger("getHit");
        }
        else
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        CurrentHp += amount;
        if (CurrentHp > MaxHp) CurrentHp = MaxHp;
        UpdateHealthUI();
    }

    public void HealToFull()
    {
        CurrentHp = MaxHp;
        UpdateHealthUI();
    }

    private void Die()
    {
        isDead = true;
        CurrentHp = 0;
        amin.SetTrigger("die");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(GameState.DIE);
        }
    }

    public bool IsDead() => isDead;

    // --- QUẢN LÝ UI ---
    private void UpdateAllUI()
    {
        UpdateHealthUI();
        UpdateLevelUI();
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = MaxHp;
            healthBar.value = CurrentHp;
        }
    }

    private void UpdateLevelUI()
    {
        if (xpBar != null)
        {
            xpBar.maxValue = RequiredXp;
            xpBar.value = CurrentXp;
        }
        if (levelText != null)
        {
            levelText.text = "LV: " + Level;
        }
    }
}