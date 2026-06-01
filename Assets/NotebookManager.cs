using UnityEngine;
using TMPro; // 引入 TextMeshPro 功能
using UnityEngine.SceneManagement; // 引入場景管理功能，用來判斷現在在哪個場景

public class NotebookManager : MonoBehaviour
{
    [Header("筆記本 Canvas (NotebookCanvas)")]
    public GameObject notebookUI;

    [Header("玩家的眼睛 (CenterEyeAnchor)")]
    public Transform centerEye;

    [Header("剛剛新增的文字框 (TaskText)")]
    public TextMeshProUGUI notebookText;

    // ==========================================
    // 讓你可以直接在 Unity 面板微調位置
    // ==========================================
    [Header("UI 顯示位置微調")]
    [Tooltip("筆記本距離眼睛多遠 (預設 0.6)")]
    public float uiDistance = 0.6f;
    [Tooltip("筆記本的高度偏移：0是正中間，負數是往下，正數是往上 (預設 -0.05)")]
    public float uiHeightOffset = -0.05f; 

    private bool isOpen = false;

    void Start()
    {
        // 遊戲一開始隱藏筆記本
        if (notebookUI != null) notebookUI.SetActive(false);
    }

    void Update()
    {
        // ==========================================
        // 安全鎖 1：如果還在 Init (初始/主選單) 場景，直接封鎖 B 鍵功能
        // ==========================================
        if (SceneManager.GetActiveScene().name == "Init")
        {
            ForceCloseNotebook();
            return; // 提早結束，不執行下方的按鍵偵測
        }

        // ==========================================
        // 安全鎖 2：【新增】如果遊戲已經進入結局階段，永久封鎖筆記本
        // ==========================================
        if (GameFlowManager.instance != null && GameFlowManager.instance.isGameEnding)
        {
            ForceCloseNotebook();
            return; // 提早結束，不執行下方的按鍵偵測
        }

        // 偵測右手 B 鍵
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            isOpen = !isOpen;
            
            if (isOpen)
            {
                // 每次打開筆記本的瞬間，都重新檢查並更新文字狀態！
                UpdateNotebookText();
            }
            
            if (notebookUI != null) notebookUI.SetActive(isOpen);
        }

        // ==========================================
        // 只要筆記本是打開的狀態，每一幀都強制跟隨玩家視角
        // ==========================================
        if (isOpen && notebookUI != null && centerEye != null)
        {
            // 套用你在 Unity 面板設定的距離與高度數字
            notebookUI.transform.position = centerEye.position + centerEye.forward * uiDistance + centerEye.up * uiHeightOffset;
            notebookUI.transform.LookAt(centerEye);
            notebookUI.transform.Rotate(0, 180, 0); 
        }
    }

    // ==========================================
    // 【新增】：將強制關閉獨立寫成一個功能，方便安全鎖呼叫
    // ==========================================
    private void ForceCloseNotebook()
    {
        if (isOpen)
        {
            isOpen = false;
            if (notebookUI != null) notebookUI.SetActive(false);
        }
    }

    // ==========================================
    // 動態更新筆記本文字與刪除線
    // ==========================================
    public void UpdateNotebookText()
    {
        if (notebookText == null || GameFlowManager.instance == null) return;

        // 檢查大腦，判斷各項物品是否已經收集？
        // 如果有，就打個 [V] 並用 <s> </s> 標籤包起來產生刪除線！
        string t1 = GameFlowManager.instance.IsItemCollected("TransferForm_Whole_Board") 
            ? "<s>[V] 調查走廊佈告欄與周邊環境。</s>" 
            : "[ ] 調查走廊佈告欄與周邊環境。";

        string t2 = GameFlowManager.instance.IsItemCollected("TransferForm_Classroom") 
            ? "<s>[V] 清查教室內部。</s>" 
            : "[ ] 清查教室內部。";

        string t3 = GameFlowManager.instance.IsItemCollected("book_locker") 
            ? "<s>[V] 檢查校內置物空間與淋浴間設施。</s>" 
            : "[ ] 檢查校內置物空間與淋浴間設施。";

        string t4 = GameFlowManager.instance.IsItemCollected("book_chair") 
            ? "<s>[V] 巡視大禮堂與體育館。</s>" 
            : "[ ] 巡視大禮堂與體育館。";

        // 把所有文字組合起來，印到畫面上 (<b> 代表粗體 )
        // 最下方的【其他注意事項】是獨立寫死的字串，所以絕對不會被加上刪除線
        notebookText.text = 
            "<b>【校園巡視清單】</b>\n\n" + 
            t1 + "\n" + 
            t2 + "\n" + 
            t3 + "\n" + 
            t4 + "\n\n" +
            "----------------------------------\n" +
            "<b>【其他注意事項】</b>\n" +
            "體育館的散落球具尚未歸位，請留意場地安全。";
    }
}