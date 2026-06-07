using UnityEngine;
using System.Collections;

public class SequentialItemManager : MonoBehaviour
{
    [Header("【第二局防呆】防止背包吃掉物品的重生機制")]
    [Tooltip("拉入 book_locker 的 Prefab (從 Project)")]
    public GameObject prefab_book_locker;
    [Tooltip("拉入書本重生的空座標點 (從 Hierarchy)")]
    public Transform spawnPoint_book_locker; 

    [Space(10)]
    [Tooltip("拉入 TransferForm_Whole_bad 的 Prefab (從 Project)")]
    public GameObject prefab_TransferForm_Whole_bad;
    [Tooltip("拉入隱藏表單重生的空座標點 (從 Hierarchy)")]
    public Transform spawnPoint_TransferForm_Whole_bad;

    [Header("【第一局用】請把場景裡的物品拖進來 (從 Hierarchy)")]
    public GameObject item_book_locker; 
    public GameObject item_TransferForm_Whole_Board;
    public GameObject item_book_chair;
    public GameObject item_TransferForm_Classroom;
    public GameObject item_TransferForm_Whole_bad;

    private bool hiddenItemSpawned = false;
    private float respawnCooldown = 0f; // 重生冷卻時間，防止瞬間無限迴圈卡死

    void OnEnable()
    {
        StartCoroutine(DelayedSync());
    }

    IEnumerator DelayedSync()
    {
        while (GameFlowManager.instance == null) yield return null;
        
        // 給系統與物理引擎 0.2 秒的時間去穩定下來，避免出生瞬間碰撞爆炸
        yield return new WaitForSeconds(0.2f);

        // ==========================================
        // 【護航 1】：安全復活 book_locker
        // ==========================================
        if (item_book_locker == null && prefab_book_locker != null && spawnPoint_book_locker != null)
        {
            // 生成後先強制隱藏，躲過第一波掃描與碰撞！
            item_book_locker = Instantiate(prefab_book_locker, spawnPoint_book_locker.position, spawnPoint_book_locker.rotation);
            item_book_locker.name = "book_locker"; 
            item_book_locker.SetActive(false); 
        }

        // ==========================================
        // 【護航 2】：安全復活 TransferForm_Whole_bad
        // ==========================================
        if (item_TransferForm_Whole_bad == null && prefab_TransferForm_Whole_bad != null && spawnPoint_TransferForm_Whole_bad != null)
        {
            item_TransferForm_Whole_bad = Instantiate(prefab_TransferForm_Whole_bad, spawnPoint_TransferForm_Whole_bad.position, spawnPoint_TransferForm_Whole_bad.rotation);
            item_TransferForm_Whole_bad.name = "TransferForm_Whole_bad";
            item_TransferForm_Whole_bad.SetActive(false); 
        }

        hiddenItemSpawned = false;
    }

    IEnumerator Start()
    {
        while (GameFlowManager.instance == null) yield return null;
        
        // 初始化其他物品的隱藏狀態 (不包含上面已經處理過的)
        if (item_TransferForm_Whole_Board != null) item_TransferForm_Whole_Board.SetActive(false);
        if (item_book_chair != null) item_book_chair.SetActive(false);
        if (item_TransferForm_Classroom != null) item_TransferForm_Classroom.SetActive(false);
    }

    void Update()
    {
        if (GameFlowManager.instance == null) return;

        // ==========================================
        // 【終極暴力無限復活】：對抗背包系統的秒殺機制！
        // ==========================================
        if (!GameFlowManager.instance.IsItemCollected("book_locker"))
        {
            if (item_book_locker == null) // 如果變成 Missing (被背包秒殺了)
            {
                respawnCooldown -= Time.deltaTime;
                if (respawnCooldown <= 0f)
                {
                    if (prefab_book_locker != null && spawnPoint_book_locker != null)
                    {
                        item_book_locker = Instantiate(prefab_book_locker, spawnPoint_book_locker.position, spawnPoint_book_locker.rotation);
                        item_book_locker.name = "book_locker";
                        item_book_locker.SetActive(true); // 既然系統穩定了，直接叫出來
                        Debug.LogWarning("⚠️ 系統護航：偵測到 book_locker 變成 Missing！已強制原地重生！");
                        respawnCooldown = 0.5f; // 給 0.5 秒的冷卻，避免瞬間卡死
                    }
                }
            }
            else
            {
                // 如果沒被殺掉，確保它是顯示的
                if (!item_book_locker.activeSelf) item_book_locker.SetActive(true);
            }
        }
        else
        {
            // 如果大腦說已經撿過，確保它關閉
            if (item_book_locker != null && item_book_locker.activeSelf) item_book_locker.SetActive(false);
        }

        // ==========================================
        // 喚醒後續連鎖物件
        // ==========================================

        // 1. 喚醒 Board
        if (item_TransferForm_Whole_Board != null)
        {
            bool shouldShowBoard = GameFlowManager.instance.IsItemCollected("book_locker") && !GameFlowManager.instance.IsItemCollected("TransferForm_Whole_Board");
            if (shouldShowBoard && !item_TransferForm_Whole_Board.activeSelf)
                item_TransferForm_Whole_Board.SetActive(true);
        }

        // 2. 喚醒 Chair
        if (item_book_chair != null)
        {
            bool shouldShowChair = GameFlowManager.instance.IsItemCollected("TransferForm_Whole_Board") && !GameFlowManager.instance.IsItemCollected("book_chair");
            if (shouldShowChair && !item_book_chair.activeSelf)
                item_book_chair.SetActive(true);
        }

        // 3. 喚醒 Classroom
        if (item_TransferForm_Classroom != null)
        {
            bool shouldShowClassroom = GameFlowManager.instance.IsItemCollected("book_chair") && !GameFlowManager.instance.IsItemCollected("TransferForm_Classroom");
            if (shouldShowClassroom && !item_TransferForm_Classroom.activeSelf)
                item_TransferForm_Classroom.SetActive(true);
        }

        // 4. 喚醒隱藏結局物件 (同樣加入防秒殺護航)
        if (GameFlowManager.instance.HasCollectedAllRequiredItems() && !hiddenItemSpawned)
        {
            // 如果準備要生出來，卻發現 Missing，立刻補救一次
            if (item_TransferForm_Whole_bad == null && prefab_TransferForm_Whole_bad != null && spawnPoint_TransferForm_Whole_bad != null)
            {
                item_TransferForm_Whole_bad = Instantiate(prefab_TransferForm_Whole_bad, spawnPoint_TransferForm_Whole_bad.position, spawnPoint_TransferForm_Whole_bad.rotation);
                item_TransferForm_Whole_bad.name = "TransferForm_Whole_bad";
                item_TransferForm_Whole_bad.SetActive(false);
            }

            if (item_TransferForm_Whole_bad != null)
            {
                hiddenItemSpawned = true;
                StartCoroutine(SpawnHiddenItemAfterAudio());
            }
        }
    }

    IEnumerator SpawnHiddenItemAfterAudio()
    {
        if (GameFlowManager.instance.audioSource != null)
        {
            yield return new WaitForSeconds(0.5f);
            // 等待第四個物品的收集語音播完
            yield return new WaitWhile(() => GameFlowManager.instance.audioSource.isPlaying);
        }

        // 呼叫大腦播放隱藏任務開啟音效！
        GameFlowManager.instance.PlayHiddenTaskUnlockSound();

        GameFlowManager.instance.isHiddenTaskRevealed = true;

        NotebookManager notebook = FindObjectOfType<NotebookManager>();
        if (notebook != null) notebook.UpdateNotebookImage();

        // 立刻顯示 Bad 轉學單，如果玩家手速快秒撿，GameFlowManager 的 audioSource.Stop() 就會發動，直接咖掉上面的解鎖音效！
        if (item_TransferForm_Whole_bad != null)
            item_TransferForm_Whole_bad.SetActive(true);
    }
}