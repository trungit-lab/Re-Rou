// PlayerMovement.cs (Phiên bản đã sửa, thống nhất)
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    // === Components & Input ===
    private CharacterController ccl;
    private Animator amin;
    private Vector2 moveInput;

    [Header("Movement Stats")]
    public float movementSpeed = 5f;
    public float rotationSpeed = 10f; // Tốc độ xoay của nhân vật

    // *** SỬA ĐỔI 1: THÊM BIẾN THAM CHIẾU CAMERA ***
    [Header("Dependencies")]
    [Tooltip("Kéo camera chính của bạn vào đây. Hệ thống di chuyển sẽ dựa vào hướng của camera này.")]
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

        // Thêm một cảnh báo nếu bạn quên gán camera
        if (mainCameraTransform == null)
        {
            Debug.LogError("Vui lòng gán Main Camera Transform vào script PlayerMovement trong Inspector!");
        }
    }

    private void Update()
    {
        HandleGravity();
        HandleMovement();
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    // *** SỬA ĐỔI 2: VIẾT LẠI HOÀN TOÀN HÀM HandleMovement ***
    // Đã xóa bỏ toàn bộ khối #if để tạo ra một logic duy nhất
    private void HandleMovement()
    {
        // Tính hướng di chuyển dựa vào hướng camera
        Vector3 camForward = mainCameraTransform.forward;
        Vector3 camRight = mainCameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x);

        // --- Di chuyển ---
        ccl.Move(moveDirection.normalized * movementSpeed * Time.deltaTime);

        // --- Xoay nhân vật ---
        if (moveInput.magnitude > 0.1f)
        {
            // Nếu đang đi lùi (input.y < 0), không quay 180 độ
            Vector3 lookDir;
            if (moveInput.y < 0)
            {
                // Luôn giữ hướng nhìn cùng hướng với camera (đi lùi mà không quay đầu)
                lookDir = new Vector3(camForward.x, 0, camForward.z);
            }
            else
            {
                lookDir = new Vector3(moveDirection.x, 0, moveDirection.z);
            }

            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // --- Cập nhật Animator ---
        amin.SetBool("isWalk", moveInput.magnitude > 0.1f);
    }

    private void HandleGravity()
    {
        // isGrounded được set thành true trong OnControllerColliderHit
        if (isGrounded)
        {
            // Reset số lần nhảy và giảm nhẹ trọng lực khi chạm đất để nhân vật "dính" đất hơn
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
        // Cho phép nhảy ngay cả khi đang rơi (double jump)
        if (jumpCount < maxJumps)
        {
            isGrounded = false; // Ngay khi nhảy, không còn ở trên mặt đất
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

    // Luôn luôn reset isGrounded ở đầu mỗi frame để kiểm tra lại
    private void FixedUpdate()
    {
        isGrounded = false;
    }

    // Dùng OnControllerColliderHit để kiểm tra va chạm đất một cách đáng tin cậy
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Kiểm tra va chạm với các object có tag "Ground" và bề mặt va chạm phải hướng lên
        if (hit.gameObject.CompareTag("Ground") && hit.normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }
}