using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // === Dependencies & Components ===
    private GameManager _gm;
    private CharacterController ccl;
    private Animator amin;
    public Transform cameraTransform;

    [Header("Player Stats")]
    public float movementSpeed = 5f;
    public float mouseSensitivity = 150f;

    [Header("Camera Look")]
    private float xRotation = 0f;
    public float lookXLimit = 80f;

    [Header("Jump Config")]
    public float jumpHeight = 2f;
    private Vector3 velocity;
    private bool isGrounded;
    private float gravity = -19.62f;

    #region Variable Declarations
    [Header("HP & HealthBar")]
    private const float HP = 10;
    public float hp = HP;
    public Slider healthBar;

    [Header("Attack Config")]
    public ParticleSystem attackEffect;
    public Transform hitBox;
    [Range(0.2f, 2f)]
    public float hitRange = 0.5f;
    private bool isAttacking = false;
    public LayerMask hitMask;
    public float dmg = 1;

    [Header("Dash Config")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    private bool canDash = true;
    #endregion


    void Start()
    {
        _gm = FindObjectOfType<GameManager>();
        ccl = GetComponent<CharacterController>();
        amin = GetComponent<Animator>();
        hp = HP;
        canDash = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (_gm.gameState != GameState.GAMEPLAY) { return; }
        HandleJumpAndGravity();
        HandleRotation();
        HandleMovement();
        HandleInput();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Ground"))
        {
            if (hit.normal.y > 0.5f)
            {
                isGrounded = true;
            }
        }
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -lookXLimit, lookXLimit);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 moveDirection = transform.right * x + transform.forward * z;
        ccl.Move(moveDirection.normalized * movementSpeed * Time.deltaTime);
        amin.SetBool("isWalk", moveDirection.magnitude > 0.1f);
    }

    private void HandleJumpAndGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        ccl.Move(velocity * Time.deltaTime);
        isGrounded = false;
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            Debug.Log("Attack");
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.F) && canDash)
        {
            StartCoroutine(Dash());
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            amin.SetTrigger("isJump");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void Attack()
    {
        if (attackEffect != null) attackEffect.Play();
        // isAttacking = true; // Tạm thời comment dòng này
        amin.SetTrigger("isAttack");

        // Bắt đầu một coroutine để reset trạng thái tấn công sau 0.5 giây
        StartCoroutine(AttackCooldown());

        Collider[] hitInfor = Physics.OverlapSphere(hitBox.position, hitRange, hitMask);
        foreach (Collider c in hitInfor)
        {
            c.gameObject.SendMessage("GetHit", dmg, SendMessageOptions.DontRequireReceiver);
        }
    }

    private IEnumerator AttackCooldown()
    {
        isAttacking = true; // Đặt isAttacking thành true ở ĐẦU coroutine
        yield return new WaitForSeconds(0.5f); // Chờ 0.5 giây (hoặc thời gian animation của bạn)
        isAttacking = false; // Đặt lại thành false
        Debug.Log("Attack Cooldown Finished. Can attack again!"); // Thêm log để xác nhận
    }
    //public void OnAttackEnd() // THÊM "public" VÀO ĐÂY
    //{
    //    isAttacking = false;
    //}


    private IEnumerator Dash()
    {
        canDash = false;
        amin.SetTrigger("isDash");
        float startTime = Time.time;
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 dashDirection = (transform.right * x + transform.forward * z).normalized;
        if (dashDirection.sqrMagnitude < 0.1f)
        {
            dashDirection = transform.forward;
        }
        while (Time.time < startTime + dashDuration)
        {
            ccl.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void GetHit(int amount)
    {
        if (hp <= 0) return;
        hp -= amount;
        healthBar.value = hp / HP;
        if (hp > 0)
        {
            amin.SetTrigger("getHit");
        }
        else
        {
            hp = 0;
            amin.SetTrigger("die");
            _gm.ChangeGameState(GameState.DIE);
            StartCoroutine(TimeIsDie());
        }
    }

    IEnumerator TimeIsDie()
    {
        yield return new WaitForSeconds(2f);
        _gm.DieDone();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Slime")) GetHit(1);
        if (other.gameObject.CompareTag("Diamond"))
        {
            Destroy(other.gameObject);
            _gm.Plus();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (hitBox != null) Gizmos.DrawWireSphere(hitBox.position, hitRange);
    }
}