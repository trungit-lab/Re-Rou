// PlayerLook.cs (Phiên bản Thống nhất - Hoạt động cho cả PC và Mobile)
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    // --- Input Storage ---
    private Vector2 lookInput_Default;
    private Vector2 lookInput_FreeLook;

    // --- State Control ---
    private bool isFreeLookEnabled = false;
    private bool isCursorUnlocked = false;

    [Header("Look Stats")]
    [Tooltip("Độ nhạy cho cả Chuột và Cảm ứng. Bạn sẽ cần điều chỉnh giá trị này để phù hợp với cả hai.")]
    public float sensitivityX = 150f;
    [Tooltip("Độ nhạy cho cả Chuột và Cảm ứng. Bạn sẽ cần điều chỉnh giá trị này để phù hợp với cả hai.")]
    public float sensitivityY = 100f;
    public Transform cameraTransform;

    [Header("Camera Look Limit")]
    private float xRotation = 20f;
    public float lookXLimitMin = -45f;
    public float lookXLimitMax = 30f;

    private void Start()
    {
        SetCursorLock(false);
    }

    private void Update()
    {
        if (isCursorUnlocked) return;
        HandleLook();
    }

    #region Public Setters (Called by PlayerController)

    public void SetDefaultLookInput(Vector2 input)
    {
        if (!isFreeLookEnabled) { lookInput_Default = input; }
        else { lookInput_Default = Vector2.zero; }
    }

    public void SetFreeLookInput(Vector2 input)
    {
        if (isFreeLookEnabled) { lookInput_FreeLook = input; }
    }

    public void SetFreeLookEnabled(bool isEnabled)
    {
        isFreeLookEnabled = isEnabled;
        if (!isEnabled) { SyncCameraOnExitFreeLook(); }
    }

    public void SetCursorLock(bool isPressed)
    {
        isCursorUnlocked = isPressed;
        Cursor.lockState = isPressed ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPressed;
    }

    #endregion

    #region Core Logic

    // *** ĐÂY LÀ PHẦN SỬA ĐỔI QUAN TRỌNG NHẤT ***
    // Đã xóa hết mọi logic phân biệt nền tảng.
    private void HandleLook()
    {
        Vector2 activeLookInput = isFreeLookEnabled ? lookInput_FreeLook : lookInput_Default;

        if (activeLookInput.sqrMagnitude < 0.01f) return;

   
        float finalX = activeLookInput.x * sensitivityX * Time.deltaTime;
        float finalY = activeLookInput.y * sensitivityY * Time.deltaTime;

        xRotation -= finalY;
        xRotation = Mathf.Clamp(xRotation, lookXLimitMin, lookXLimitMax);

        if (isFreeLookEnabled)
        {
            cameraTransform.Rotate(Vector3.up * finalX, Space.World);
            cameraTransform.rotation = Quaternion.Euler(xRotation, cameraTransform.eulerAngles.y, 0);
        }
        else
        {
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * finalX);
        }
    }

    private void SyncCameraOnExitFreeLook()
    {
        float cameraYaw = cameraTransform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0, cameraYaw, 0);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }

    #endregion
}