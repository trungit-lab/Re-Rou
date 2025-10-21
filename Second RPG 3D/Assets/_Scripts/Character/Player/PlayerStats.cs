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
        if (isDead) return;

        currentHp -= amount;
        if (healthBar != null) healthBar.value = currentHp;

        if (currentHp > 0)
        {
            amin.SetTrigger("getHit");
        }
        else
        {
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