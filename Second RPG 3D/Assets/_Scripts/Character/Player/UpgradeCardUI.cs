// FILE: UpgradeCardUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class UpgradeCardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text upgradeNameText;
    public TMP_Text upgradeDescriptionText;
    public TMP_Text valueText; // Hiển thị số liệu (VD: +5)
    public Button selectButton;
    public Image iconImage;

    private UpgradeData myData;
    private Action<UpgradeData> onSelectCallback;

    // Hàm này được Controller gọi để đổ dữ liệu vào thẻ
    public void Setup(UpgradeData data, Action<UpgradeData> callback)
    {
        Debug.Log($"1. Setup đang chạy cho thẻ: {gameObject.name}");
        myData = data;
        onSelectCallback = callback;

        // Cập nhật text
        if (upgradeNameText != null) upgradeNameText.text = data.upgradeName;
        if (upgradeDescriptionText != null) upgradeDescriptionText.text = data.upgradeDescription;

        if (valueText != null) valueText.text = "+" + data.value.ToString();

        if (iconImage != null)
        {
            if (data.icon != null)
            {
                iconImage.sprite = data.icon; // Thay đổi ảnh sprite
                iconImage.gameObject.SetActive(true); // Hiện ảnh lên
            }
            else
            {
                // Nếu data không có ảnh thì ẩn khung ảnh đi cho đỡ xấu
                iconImage.gameObject.SetActive(false);
            }
        }

        // Gán sự kiện click
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnButtonClick);
        Debug.Log($"Đã gắn nút bấm vào hàm nội bộ tên là: 'OnButtonClick'");
    }

    [ContextMenu("TEST CLICK GIẢ (Bấm vào đây)")]
    private void OnButtonClick()
    {
        Debug.Log($"[CLICK] Bạn vừa bấm vào thẻ: {upgradeNameText.text}");

        if (onSelectCallback != null && myData != null)
        {
            Debug.Log("-> Đang gửi dữ liệu về LevelUpUIController...");
            onSelectCallback.Invoke(myData);
        }
        else
        {
            Debug.LogError("Lỗi: Không có Callback hoặc Data bị Null!");
        }
    }

   
}