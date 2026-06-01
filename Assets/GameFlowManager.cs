using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; 

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager instance;

    [Header("測試專用")]
    public bool autoStartTimer = false; 

    [Header("計時器設定")]
    [Tooltip("遊戲總時間 (20分鐘 = 1200秒)")]
    public float timeRemaining = 1200f; 
    public TextMeshProUGUI timerText; 
    private bool isTimerRunning = false;

    [Header("結局與轉場 UI")]
    public Image fadeOverlay;
    public GameObject ending1Panel; 
    public GameObject ending2Panel; 
    public GameObject ending3Panel; 

    // ==========================================
    // 【新增】：收集語音設定
    // ==========================================
    [Header("收集語音設定")]
    public AudioSource audioSource;
    [Tooltip("對應 book_chair")]
    public AudioClip sound_book_chair;
    [Tooltip("對應 TransferForm_Whole_Board")]
    public AudioClip sound_TransferForm_Whole_Board;
    [Tooltip("對應 TransferForm_Classroom")]
    public AudioClip sound_TransferForm_Classroom;
    [Tooltip("對應 TransferForm_Whole_bad")]
    public AudioClip sound_TransferForm_Whole_bad;
    [Tooltip("對應 book_locker (以防萬一你也想加)")]
    public AudioClip sound_book_locker;

    private HashSet<string> collectedItems = new HashSet<string>();

    private string[] requiredItems = new string[] 
    {
        "TransferForm_Whole_Board",
        "TransferForm_Classroom",
        "book_chair",
        "book_locker"
    };

    private string hiddenEndingItem = "TransferForm_Whole_bad";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "whole")
        {
            StartTimer();
        }
    }

    void Start()
    {
        if (ending1Panel != null) ending1Panel.SetActive(false);
        if (ending2Panel != null) ending2Panel.SetActive(false);
        if (ending3Panel != null) ending3Panel.SetActive(false);
        
        if (timerText != null) timerText.gameObject.SetActive(false);

        // 自動抓取身上的 AudioSource 元件
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (autoStartTimer) StartTimer(); 
    }

    void Update()
    {
        if (!isTimerRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            isTimerRunning = false;

            if (HasCollectedAllRequiredItems())
            {
                TriggerEnding(1); 
            }
            else
            {
                TriggerEnding(2); 
            }
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60F);
            int seconds = Mathf.FloorToInt(timeRemaining - minutes * 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void StartTimer()
    {
        isTimerRunning = true;
        if (timerText != null) timerText.gameObject.SetActive(true);
    }

    public void CollectItem(string itemName)
    {
        if (!isTimerRunning) return;

        collectedItems.Add(itemName);

        // ==========================================
        // 判斷物品名稱，並播放對應音檔
        // ==========================================
        if (audioSource != null)
        {
            if (itemName == "book_chair" && sound_book_chair != null)
                audioSource.PlayOneShot(sound_book_chair);
                
            else if (itemName == "TransferForm_Whole_Board" && sound_TransferForm_Whole_Board != null)
                audioSource.PlayOneShot(sound_TransferForm_Whole_Board);
                
            else if (itemName == "TransferForm_Classroom" && sound_TransferForm_Classroom != null)
                audioSource.PlayOneShot(sound_TransferForm_Classroom);
                
            else if (itemName == "TransferForm_Whole_bad" && sound_TransferForm_Whole_bad != null)
                audioSource.PlayOneShot(sound_TransferForm_Whole_bad);
                
            else if (itemName == "book_locker" && sound_book_locker != null)
                audioSource.PlayOneShot(sound_book_locker);
        }
        
        if (HasCollectedAllRequiredItems() && collectedItems.Contains(hiddenEndingItem))
        {
            TriggerEnding(3); 
        }
    }

    // ==========================================
    // 讓筆記本可以來詢問某個特定物品撿到了沒
    // ==========================================
    public bool IsItemCollected(string itemName)
    {
        return collectedItems.Contains(itemName);
    }

    // ==========================================
    // 讓隱藏道具的腳本可以隨時問大腦「玩家找齊四個了嗎？」
    // ==========================================
    public bool HasCollectedAllRequiredItems()
    {
        foreach (string itemName in requiredItems)
        {
            if (!collectedItems.Contains(itemName))
            {
                return false; 
            }
        }
        return true; 
    }

    private void TriggerEnding(int endingType)
    {
        isTimerRunning = false; 
        StartCoroutine(EndingRoutine(endingType));
    }

    IEnumerator EndingRoutine(int endingType)
    {
        // 1. 癱瘓玩家的移動能力 (關閉重力與移動控制)
        OVRPlayerController playerCtrl = FindObjectOfType<OVRPlayerController>();
        if (playerCtrl != null) playerCtrl.enabled = false;

        CharacterController charCtrl = FindObjectOfType<CharacterController>();
        if (charCtrl != null) charCtrl.enabled = false;

        // 2. 跨場景尋找黑畫面
        if (fadeOverlay == null)
        {
            GameObject fadeObj = GameObject.Find("FadeOverlay");
            if (fadeObj != null) fadeOverlay = fadeObj.GetComponent<Image>();
        }

        // 3. 漸變黑畫面 (Fade Out) - 確保畫面完全黑掉！
        if (fadeOverlay != null)
        {
            float timer = 0f;
            float fadeDuration = 1.5f;
            Color c = fadeOverlay.color;
            
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime; 
                float alpha = Mathf.Lerp(c.a, 1, timer / fadeDuration);
                fadeOverlay.color = new Color(c.r, c.g, c.b, alpha);
                yield return null;
            }
            fadeOverlay.color = new Color(0, 0, 0, 1);
        }

        // 4. 等畫面全黑後，才瞬間傳送到虛空
        GameObject globalPlayer = GameObject.Find("[Global_Player]");
        if (globalPlayer != null)
        {
            globalPlayer.transform.position = new Vector3(0, 1000f, 0);
        }

        // 5. 強制喚醒雷射筆
        if (VRLaserToggle.instance != null)
        {
            VRLaserToggle.instance.SetLaserState(true);
        }

        // 6. 根據條件開啟對應的結局面板
        GameObject activeEndingPanel = null;
        if (endingType == 1) activeEndingPanel = ending1Panel;
        else if (endingType == 2) activeEndingPanel = ending2Panel;
        else if (endingType == 3) activeEndingPanel = ending3Panel;

        // 7. 將面板移到眼前
        if (activeEndingPanel != null)
        {
            GameObject camObj = GameObject.Find("CenterEyeAnchor");
            if (camObj != null)
            {
                Transform playerEyes = camObj.transform;
                
                // ==========================================
                // 【關鍵修改】：距離退回到 2.5 公尺，減輕視覺壓迫感
                // ==========================================
                activeEndingPanel.transform.position = playerEyes.position + playerEyes.forward * 2.5f;
                activeEndingPanel.transform.LookAt(playerEyes);
                activeEndingPanel.transform.Rotate(0, 180, 0); 
                
                // 解除雙重縮小限制，完全套用 Unity 介面中設計的大小
                activeEndingPanel.transform.localScale = new Vector3(1f, 1f, 1f);
            }

            activeEndingPanel.SetActive(true);
        }
    }
}