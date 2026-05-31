using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class VRBackpack : MonoBehaviour
{
    [Header("玩家的眼睛 (CenterEyeAnchor)")]
    public Transform centerEyeCamera; 
    
    [Header("剛剛做好的提示畫布 (NotifyCanvas)")]
    public GameObject uiCanvas; 
    
    [Header("背包清單 (不用動它)")]
    public List<GameObject> inventory = new List<GameObject>();

    private bool isItemOut = false;
    private int currentIndex = 0;

    void Start()
    {
        // 確保遊戲開始時提示 UI 是隱藏的
        if (uiCanvas != null) uiCanvas.SetActive(false);
    }

    void Update()
    {
        // 如果背包是空的，按鈕無效
        if (inventory.Count == 0) return;

        // 【左手 Y 鍵】：打開背包 / 收起道具
        if (OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            isItemOut = !isItemOut;
            RefreshDisplay();
        }

        // 【左手 X 鍵】：切換下一個道具
        if (isItemOut && inventory.Count > 1 && OVRInput.GetDown(OVRInput.RawButton.X))
        {
            if (inventory[currentIndex] != null) inventory[currentIndex].SetActive(false);
            currentIndex = (currentIndex + 1) % inventory.Count;
            RefreshDisplay();
        }
    }

    // 讓道具呼叫的「收件程式」
    public void AddItem(GameObject item)
    {
        if (!inventory.Contains(item))
        {
            inventory.Add(item);

            // 跨場景物品記憶連線
            ItemStateSaver saver = item.GetComponent<ItemStateSaver>();
            if (saver != null)
            {
                saver.MarkAsCollected();
            }

            // 跨場景道具保留優化（認爸爸機制）
            item.transform.SetParent(this.transform);

            item.SetActive(false); // 收進背包，在場景中隱藏
            currentIndex = inventory.Count - 1; // 游標切換到最新道具
            
            // 顯示 UI 提示 2 秒鐘
            StopAllCoroutines();
            StartCoroutine(ShowUIRoutine());

            // ==========================================
            // 【關鍵修改】：通知遊戲大腦 (GameFlowManager) 物品已收集！
            // 這裡把 item.name 傳過去，讓大腦可以比對是不是指定的關鍵道具
            // ==========================================
            if (GameFlowManager.instance != null)
            {
                GameFlowManager.instance.CollectItem(item.name);
            }
        }
    }

    IEnumerator ShowUIRoutine()
    {
        if (uiCanvas != null)
        {
            uiCanvas.SetActive(true);
            yield return new WaitForSeconds(2f); // 顯示 2 秒
            uiCanvas.SetActive(false);
        }
    }

    void RefreshDisplay()
    {
        // 先把所有東西隱藏
        foreach (var item in inventory)
        {
            if (item != null) item.SetActive(false);
        }

        // 如果要拿出道具，就把目前選中的秀出來
        if (isItemOut && inventory[currentIndex] != null)
        {
            GameObject item = inventory[currentIndex];
            item.SetActive(true);
            
            // 預設展示距離
            float distance = 0.4f; 
            
            // ==========================================
            // 【核心邏輯】：讀取物品專屬設定
            // ==========================================
            VRItemSettings settings = item.GetComponent<VRItemSettings>();

            if (settings != null)
            {
                if (settings.keepOriginalScale)
                {
                    // 若勾選保留原尺寸，則套用遊戲開始時記錄的大小
                    item.transform.localScale = settings.storedScale;
                }
                else
                {
                    // 否則套用自訂的縮小尺寸
                    item.transform.localScale = settings.customScale;
                }
                distance = settings.displayDistance;
            }
            else
            {
                // 防呆機制：如果忘記掛載設定腳本，預設套用轉學單的縮小尺寸
                item.transform.localScale = new Vector3(0.1960359f, 0.2764107f, 0.1960359f);
            }

            // 放在眼前指定距離處
            item.transform.position = centerEyeCamera.position + centerEyeCamera.forward * distance;
            item.transform.LookAt(centerEyeCamera);
            item.transform.Rotate(0, 180, 0); // 轉正

            // 防止拿出時直接掉落 / 切換道具掉出
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }
}