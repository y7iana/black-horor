using UnityEngine;
using UnityEngine.UI; // 引入 UI 影像功能 (取代原本的 TMPro)
using UnityEngine.SceneManagement; // 引入場景管理功能，用來判斷現在在哪個場景

public class NotebookManager : MonoBehaviour
{
    [Header("筆記本 Canvas (NotebookCanvas)")]
    public GameObject notebookUI;

    [Header("玩家的眼睛 (CenterEyeAnchor)")]
    public Transform centerEye;

    [Header("清單圖片顯示區 (請放入剛建立的 Image)")]
    public Image notebookImage;

    [Header("各階段清單圖檔 (請依序拖入)")]
    public Sprite img_0;
    public Sprite img_1;
    public Sprite img_2;
    public Sprite img_3;
    public Sprite img_4;
    public Sprite img_hidden_1;
    public Sprite img_hidden_2;

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
        // 安全鎖 2：如果遊戲已經進入結局階段，永久封鎖筆記本
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
                // 每次打開筆記本的瞬間，都重新檢查並更新圖片狀態！
                UpdateNotebookImage();
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
    // 將強制關閉獨立寫成一個功能，方便安全鎖呼叫
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
    // 動態更新筆記本圖片 (看圖說故事核心)
    // ==========================================
    public void UpdateNotebookImage()
    {
        if (notebookImage == null || GameFlowManager.instance == null) return;

        // 越後面的進度，判斷優先級越高
        if (GameFlowManager.instance.IsItemCollected("TransferForm_Whole_bad"))
            notebookImage.sprite = img_hidden_2;
        else if (GameFlowManager.instance.isHiddenTaskRevealed)
            notebookImage.sprite = img_hidden_1;
        else if (GameFlowManager.instance.IsItemCollected("TransferForm_Classroom"))
            notebookImage.sprite = img_4;
        else if (GameFlowManager.instance.IsItemCollected("book_chair"))
            notebookImage.sprite = img_3;
        else if (GameFlowManager.instance.IsItemCollected("TransferForm_Whole_Board"))
            notebookImage.sprite = img_2;
        else if (GameFlowManager.instance.IsItemCollected("book_locker"))
            notebookImage.sprite = img_1;
        else
            notebookImage.sprite = img_0;
    }
}