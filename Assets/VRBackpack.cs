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

    // 【修改重點 1】：已經將原本的 Awake() 刪除！
    // 跨場景永生功能已經全權交給頂層的 [Global_Player] 上的 PlayerSingleton 處理。

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

            // ==========================================
            // 【關鍵修改：跨場景物品記憶連線】
            // 在物品正式進入背包的這一刻，立刻把它寫入全域筆記本！
            // ==========================================
            ItemStateSaver saver = item.GetComponent<ItemStateSaver>();
            if (saver != null)
            {
                saver.MarkAsCollected();
            }

            // 【修改重點 2：跨場景道具保留優化】
            // 取代原本的 DontDestroyOnLoad(item)，改為「認爸爸」機制！
            // 直接讓道具變成背包物件的子物件。因為大包包 [Global_Player] 已經永生不滅了，
            // 變成它子物件的道具就會自然而然跟著一起跨場景，架構最乾淨、絕對不會遺失！
            item.transform.SetParent(this.transform);

            item.SetActive(false); // 收進背包，在場景中隱藏
            currentIndex = inventory.Count - 1; // 游標切換到最新道具
            
            // 顯示 UI 提示 2 秒鐘
            StopAllCoroutines();
            StartCoroutine(ShowUIRoutine());
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
            
            // 放在眼前 30 公分處
            item.transform.position = centerEyeCamera.position + centerEyeCamera.forward * 0.3f;
            item.transform.LookAt(centerEyeCamera);
            item.transform.Rotate(0, 180, 0); // 轉正

            // 【核心修復：防止拿出時直接掉落 / 切換道具掉出】
            // 從背包拿出或切換道具時，強制重啟「時間暫停 (isKinematic = true)」，讓它乖乖飄在空中等你抓！
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }
}