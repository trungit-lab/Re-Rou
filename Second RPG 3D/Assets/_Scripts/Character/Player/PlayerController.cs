// PlayerController.cs (Phiên bản cuối cùng, triển khai logic cộng Input)
using UnityEngine;
using UnityEngine.InputSystem;



[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerLook))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerController : MonoBehaviour
{
    // === Dependencies ===
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private PlayerCombat playerCombat;

    // *** THÊM MỚI 1: BIẾN THAM CHIẾU VÀ BIẾN LƯU TRỮ ***
    [Header("Mobile UI Controls")]
    [Tooltip("Kéo đối tượng Joystick từ Hierarchy vào đây.")]
    public VariableJoystick fixedJoystick; // Tham chiếu đến joystick trên màn hình
    public GameObject mobileControlsCanvas;

    private Vector2 keyboardGamepadInput; // Biến để lưu trữ input từ WASD/Gamepad

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerLook = GetComponent<PlayerLook>();
        playerCombat = GetComponent<PlayerCombat>();

        if (fixedJoystick == null)
        {
            Debug.LogWarning("Chưa gán Fixed Joystick trong Inspector. Điều khiển trên mobile sẽ không hoạt động.");
        }
    }

    // *** THÊM MỚI 2: HÀM UPDATE ĐỂ LIÊN TỤC HỢP NHẤT INPUT ***
    private void Update()
    {
        // Bước 1: Đọc input từ joystick ảo. Nếu không có joystick, giá trị là (0,0).
        Vector2 joystickInput = Vector2.zero;
        if (fixedJoystick != null && fixedJoystick.gameObject.activeInHierarchy)
        {
            joystickInput = new Vector2(fixedJoystick.Horizontal, fixedJoystick.Vertical);
        }

        // Bước 2: Cộng input từ joystick và input từ keyboard/gamepad.
        // Đây chính là logic bạn đã đề xuất.
        Vector2 totalInput = keyboardGamepadInput + joystickInput;

        // Bước 3: Giới hạn độ lớn để tốc độ đi chéo không nhanh hơn đi thẳng.
        totalInput = Vector2.ClampMagnitude(totalInput, 1f);

        // Bước 4: Gửi kết quả cuối cùng đến script di chuyển.
        playerMovement.SetMoveInput(totalInput);
    }


    // === HÀM LẮNG NGHE SỰ KIỆN TỪ COMPONENT "PLAYER INPUT" ===

    // *** SỬA ĐỔI: HÀM OnMove GIỜ CHỈ LƯU TRỮ INPUT ***
    public void OnMove(InputAction.CallbackContext context)
    {
        // Thay vì gửi trực tiếp, hàm này chỉ cập nhật giá trị input từ Keyboard/Gamepad.
        // Hàm Update sẽ lo phần còn lại.
        keyboardGamepadInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        playerLook.SetDefaultLookInput(context.ReadValue<Vector2>());
    }

    public void OnAttack1(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerCombat.Attack(1);
        }
    }

    public void OnAttack2(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerCombat.Attack(2);
        }
    }

    public void OnAttack3(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerCombat.Attack(3);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerMovement.Jump();
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerCombat.Dash();
        }
    }

    public void OnEnableFreeLook(InputAction.CallbackContext context)
    {
        playerLook.SetFreeLookEnabled(context.ReadValueAsButton());
        playerLook.SetFreeLookInput(context.ReadValue<Vector2>());
    }

    public void OnUnlockCursor(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerLook.SetCursorLock(context.ReadValueAsButton());
        }
    }
}