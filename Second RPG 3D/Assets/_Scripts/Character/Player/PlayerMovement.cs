// FILE: PlayerMovement.cs (Phiên bản hoàn chỉnh)
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    // === Components & Input ===
    private CharacterController ccl;
    private Animator amin;
    private Vector2 moveInput;

    [Header("Movement Stats")]
    [SerializeField] private float movementSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Dependencies")]
    [Tooltip("Kéo camera chính của bạn vào đây.")]
    public Transform mainCameraTransform;

    [Header("Jump & Gravity")]
    public float jumpHeight = 2f;
    public int maxJumps = 2;
    private int jumpCount = 0;
    private Vector3 velocity;
    private bool isGrounded;
    private float gravity = -19.62f;

    private void Awake()
    {
        ccl = GetComponent<CharacterController>();
        amin = GetComponent<Animator>();

        if (mainCameraTransform == null)
        {
            Debug.LogError("Vui lòng gán Main Camera Transform vào script PlayerMovement!", gameObject);
        }
    }

    private void Update()
    {
        // --- SỬA ĐỔI: Kiểm tra chết ---
        // Nếu PlayerStats tồn tại và nhân vật đã chết, thì không làm gì cả (đứng yên tại chỗ)
        if (PlayerStats.Instance != null && PlayerStats.Instance.IsDead()) return;

        HandleGravity();
        HandleMovement();
    }

    public void SetMovementSpeed(float newSpeed)
    {
        movementSpeed = newSpeed;
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    private void HandleMovement()
    {
        if (mainCameraTransform == null) return; // An toàn

        Vector3 camForward = mainCameraTransform.forward;
        Vector3 camRight = mainCameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x);

        ccl.Move(moveDirection.normalized * movementSpeed * Time.deltaTime);

        if (moveInput.magnitude > 0.1f)
        {
            Vector3 lookDir;
            if (moveInput.y < 0)
            {
                lookDir = new Vector3(camForward.x, 0, camForward.z);
            }
            else
            {
                lookDir = new Vector3(moveDirection.x, 0, moveDirection.z);
            }

            if (lookDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        amin.SetBool("isWalk", moveInput.magnitude > 0.1f);
    }

    private void HandleGravity()
    {
        if (isGrounded)
        {
            jumpCount = 0;
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }

        velocity.y += gravity * Time.deltaTime;
        ccl.Move(velocity * Time.deltaTime);
    }

    public void Jump()
    {
        // --- SỬA ĐỔI: Không cho nhảy khi chết ---
        if (PlayerStats.Instance != null && PlayerStats.Instance.IsDead()) return;

        if (jumpCount < maxJumps)
        {
            isGrounded = false;
            jumpCount++;

            if (jumpCount == 1)
            {
                amin.SetTrigger("isJump");
            }
            else
            {
                amin.SetTrigger("isJump2");
            }

            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void FixedUpdate()
    {
        isGrounded = false;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Ground") && hit.normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }
}