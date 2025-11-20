using UnityEngine;
using UnityEngine.EventSystems;

public class HoverTest : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse đang hover vào button!");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse rời khỏi button!");
    }
}
