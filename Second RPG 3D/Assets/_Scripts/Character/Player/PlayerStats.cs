// PlayerStats.cs (Đã kết nối với GameManager)
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    // === Components & Dependencies ===
    private Animator amin;
    // Đã xóa tham chiếu trực tiếp đến GameManager

    [Header("Health System")]
    public float maxHp = 100f;
    private float currentHp;
    public Slider healthBar;

    [Header("Core Combat Stats")]
    public float baseDamage = 1f;

    // Trạng thái
    private bool isDead = false;

    private void Awake()
    {
        amin = GetComponent<Animator>();
    }

    private void Start()
    {
        // Không cần FindObjectOfType<GameManager>() nữa
        currentHp = maxHp;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHp;
            healthBar.value = currentHp;
        }
    }

    public void GetHit(float amount)
    {
        
        if (isDead)
        {
            return;
        }
       

        // KIỂM TRA 2: Máu trước và sau khi trừ
        float hpBefore = currentHp;
        currentHp -= amount;
        float hpAfter = currentHp;
        //Debug.Log($"[KIỂM TRA 2] Máu thay đổi: {hpBefore} -> {hpAfter}");

        // KIỂM TRA 3: Thanh máu (healthBar) có được gán không?
        if (healthBar != null)
        {
            healthBar.value = currentHp;
        }
        else
        {
            Debug.LogWarning("[KIỂM TRA 3] Lỗi! HealthBar chưa được gán trong Inspector.");
        }

        // KIỂM TRA 4: Logic chết và animation
        if (currentHp > 0)
        {
           
            amin.SetTrigger("getHit");
        }
        else
        {
            Debug.LogWarning("[KIỂM TRA 4] Máu đã hết. Gọi hàm Die().");
            Die();
        }

    }



    private void Die()
    {
        isDead = true;
        currentHp = 0;
        amin.SetTrigger("die");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(GameState.DIE);
        }
    }

    public bool IsDead()
    {
        return isDead;
    }
}