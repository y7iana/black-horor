using UnityEngine;

public class ForceTopUI : MonoBehaviour
{
    void Start()
    {
        // 抓取物件身上的 Canvas 組件
        Canvas myCanvas = GetComponent<Canvas>();
        
        if (myCanvas != null)
        {
            // 強制開啟覆蓋排序，並把優先級拉到最高 (999)
            myCanvas.overrideSorting = true;
            myCanvas.sortingOrder = 999;
        }
    }
}