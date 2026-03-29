using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIClickDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("=== 点击检测 ===");
            Debug.Log("EventSystem: " + (EventSystem.current != null ? "存在" : "不存在！"));

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count == 0)
            {
                Debug.Log("没有点击到任何 UI！");
            }
            else
            {
                foreach (RaycastResult result in results)
                {
                    Debug.Log("点击到: " + result.gameObject.name);
                }
            }
        }
    }
}