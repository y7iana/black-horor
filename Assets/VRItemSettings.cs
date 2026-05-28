using UnityEngine;

public class VRItemSettings : MonoBehaviour
{
    [Header("背包顯示設定")]
    [Tooltip("勾選此項，物品將在背包中維持原本的比例與大小。")]
    public bool keepOriginalScale = true;

    [Tooltip("若上方未勾選，將套用此自訂大小 (預設為轉學單縮小尺寸)。")]
    public Vector3 customScale = new Vector3(0.1960359f, 0.2764107f, 0.1960359f);

    [Tooltip("拿出來檢視時，距離相機的距離。")]
    public float displayDistance = 0.4f;

    [HideInInspector]
    public Vector3 storedScale;

    void Awake()
    {
        // 遊戲開始時，自動記錄物品最原始的大小
        storedScale = transform.localScale;
    }
}