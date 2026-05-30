using UnityEngine;
using System.Collections.Generic;

public class ItemStateSaver : MonoBehaviour
{
    [Header("給這個物品一個獨一無二的 ID (例如: TransferForm_Whole)")]
    public string uniqueID;

    // 這是全遊戲共用的「記憶筆記本」，跨場景絕對不會消失！
    public static List<string> collectedItems = new List<string>();

    void Start()
    {
        // 每次重新載入這個場景時，第一時間檢查筆記本
        if (collectedItems.Contains(uniqueID))
        {
            // 如果發現自己已經被收集過了，就立刻自我毀滅 (不重複生成)
            Destroy(gameObject);
        }
    }

    // ==========================================
    // 當玩家「確定收集」這個物品時，請呼叫這個方法！
    // ==========================================
    public void MarkAsCollected()
    {
        if (!collectedItems.Contains(uniqueID))
        {
            collectedItems.Add(uniqueID);
            Debug.Log($"[系統記錄] 物品 {uniqueID} 已被收集，下次不會再出現了！");
        }
    }
}