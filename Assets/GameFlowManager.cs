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

    [HideInInspector] 
    public bool isGameEnding = false; 

    // 記錄隱藏任務音檔是否已經播完 (給筆記本判定用)
    [HideInInspector] 
    public bool isHiddenTaskRevealed = false;

    [Header("結局與轉場 UI")]
    public Image fadeOverlay;
    public GameObject ending1Panel; 
    public GameObject ending2Panel; 
    public GameObject ending3Panel; 

    [Header("核心理念 UI (結局後觸發)")]
    public GameObject coreMessagePanel;     
    public CanvasGroup coreTextGroup;       
    public CanvasGroup returnButtonGroup;   

    private GameObject currentActiveEnding; 
    private bool canTriggerCoreMessage = false; 

    // ==========================================
    // 背景音樂 (BGM) 與自動音量調整
    // ==========================================
    [Header("背景音樂 (BGM) 設定")]
    public AudioSource bgmSource; // 專門播BGM的音響
    public AudioClip bgmClip;     // 背景音樂檔案
    [Range(0f, 1f)] public float bgmNormalVolume = 0.5f; // 平常BGM音量
    [Range(0f, 1f)] public float bgmDuckedVolume = 0.1f; // 播語音時BGM縮小的音量

    [Header("收集語音設定")]
    public AudioSource audioSource; // 專門播語音的音響
    
    [Space(10)]
    public AudioClip sound_book_chair;
    [Range(0f, 1f)] public float vol_book_chair = 1f; 

    [Space(5)]
    public AudioClip sound_TransferForm_Whole_Board;
    [Range(0f, 1f)] public float vol_TransferForm_Whole_Board = 1f;

    [Space(5)]
    public AudioClip sound_TransferForm_Classroom;
    [Range(0f, 1f)] public float vol_TransferForm_Classroom = 1f;

    [Space(5)]
    public AudioClip sound_TransferForm_Whole_bad;
    [Range(0f, 1f)] public float vol_TransferForm_Whole_bad = 1f;

    [Space(5)]
    public AudioClip sound_book_locker;
    [Range(0f, 1f)] public float vol_book_locker = 1f;

    // ==========================================
    // 隱藏任務開啟音效
    // ==========================================
    [Header("隱藏任務開啟音效")]
    public AudioClip sound_hiddenTaskUnlock;
    [Range(0f, 1f)] public float vol_hiddenTaskUnlock = 1f;

    // ==========================================
    // 三種結局專屬音檔
    // ==========================================
    [Header("結局音檔設定")]
    [Tooltip("對應 Ending 1 (收集滿4件，時間到)")]
    public AudioClip sound_EndingGood;        // 結局1 (播1次)
    
    [Tooltip("對應 Ending 2 (時間到，未收集滿4件)")]
    public AudioClip sound_EndingBad_Waves;   // 結局2 (海浪聲)
    public AudioClip sound_EndingBad_Gunshot; // 結局2 (槍聲)
    
    [Tooltip("對應 Ending 3 (收集滿4件+隱藏表單)")]
    public AudioClip sound_EndingFinal;       // 結局3 (播1次)

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

            // 確保 BGM 音響存在並播放背景音樂
            if (bgmSource == null) 
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
            }
            if (bgmClip != null)
            {
                bgmSource.clip = bgmClip;
                bgmSource.loop = true;
                bgmSource.volume = bgmNormalVolume;
                bgmSource.Play();
            }

            Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.name == "book_locker" && t.gameObject.scene.isLoaded)
                {
                    t.gameObject.SetActive(!IsItemCollected("book_locker"));
                    break;
                }
            }
            
            GameObject spawnPoint = GameObject.Find("SpawnPoint"); 
            GameObject globalPlayer = GameObject.Find("[Global_Player]");
            
            if (spawnPoint != null && globalPlayer != null)
            {
                OVRPlayerController playerCtrl = globalPlayer.GetComponent<OVRPlayerController>();
                if (playerCtrl != null) playerCtrl.enabled = false; 
                
                globalPlayer.transform.position = spawnPoint.transform.position;
                globalPlayer.transform.rotation = spawnPoint.transform.rotation;
                
                if (playerCtrl != null) playerCtrl.enabled = true;
            }
        }
    }

    void Start()
    {
        if (ending1Panel != null) ending1Panel.SetActive(false);
        if (ending2Panel != null) ending2Panel.SetActive(false);
        if (ending3Panel != null) ending3Panel.SetActive(false);
        
        if (coreMessagePanel != null) coreMessagePanel.SetActive(false); 
        
        if (timerText != null) timerText.gameObject.SetActive(false);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (autoStartTimer) StartTimer(); 
    }

    void Update()
    {
        // 遊戲進行中：自動偵測語音並壓低 BGM 音量
        if (!isGameEnding && bgmSource != null && bgmSource.isPlaying)
        {
            float targetVol = (audioSource != null && audioSource.isPlaying) ? bgmDuckedVolume : bgmNormalVolume;
            bgmSource.volume = Mathf.Lerp(bgmSource.volume, targetVol, Time.deltaTime * 3f);
        }

        if (isTimerRunning && !isGameEnding)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                isTimerRunning = false;

                if (HasCollectedAllRequiredItems())
                    TriggerEnding(1); // 結局1：收集滿4件
                else
                    TriggerEnding(2); // 結局2：未收集滿 (Bad Ending)
            }

            UpdateTimerUI();
        }

        if (canTriggerCoreMessage)
        {
            if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) || OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
            {
                canTriggerCoreMessage = false; 
                StartCoroutine(TransitionToCoreMessage());
            }
        }
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
        if (!isTimerRunning || isGameEnding) return;

        collectedItems.Add(itemName);

        if (audioSource != null)
        {
            // ==========================================
            // 【修正 1】：強制切斷上一秒正在播的音檔 (如解鎖任務的語音)
            // ==========================================
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            if (itemName == "book_chair" && sound_book_chair != null)
                audioSource.PlayOneShot(sound_book_chair, vol_book_chair);
                
            else if (itemName == "TransferForm_Whole_Board" && sound_TransferForm_Whole_Board != null)
                audioSource.PlayOneShot(sound_TransferForm_Whole_Board, vol_TransferForm_Whole_Board);
                
            else if (itemName == "TransferForm_Classroom" && sound_TransferForm_Classroom != null)
                audioSource.PlayOneShot(sound_TransferForm_Classroom, vol_TransferForm_Classroom);
                
            else if (itemName == "TransferForm_Whole_bad" && sound_TransferForm_Whole_bad != null)
                audioSource.PlayOneShot(sound_TransferForm_Whole_bad, vol_TransferForm_Whole_bad);
                
            else if (itemName == "book_locker" && sound_book_locker != null)
                audioSource.PlayOneShot(sound_book_locker, vol_book_locker);
        }

        NotebookManager nb = FindObjectOfType<NotebookManager>();
        if (nb != null) nb.UpdateNotebookImage();
        
        if (HasCollectedAllRequiredItems() && collectedItems.Contains(hiddenEndingItem))
        {
            TriggerEnding(3); // 結局3：隱藏任務觸發 (Final Ending)
        }
    }

    public bool IsItemCollected(string itemName)
    {
        return collectedItems.Contains(itemName);
    }

    public bool HasCollectedAllRequiredItems()
    {
        foreach (string itemName in requiredItems)
        {
            if (!collectedItems.Contains(itemName))
                return false; 
        }
        return true; 
    }

    public void PlayHiddenTaskUnlockSound()
    {
        if (audioSource != null && sound_hiddenTaskUnlock != null)
        {
            audioSource.PlayOneShot(sound_hiddenTaskUnlock, vol_hiddenTaskUnlock);
        }
    }

    private void TriggerEnding(int endingType)
    {
        isTimerRunning = false; 
        isGameEnding = true; 

        if (bgmSource != null) bgmSource.Stop();

        GameObject globalPlayer = GameObject.Find("[Global_Player]");
        if (globalPlayer != null)
        {
            AudioSource[] playerAudioSources = globalPlayer.GetComponentsInChildren<AudioSource>();
            foreach(AudioSource src in playerAudioSources)
            {
                src.Stop();
                src.enabled = false; 
            }
        }

        OVRPlayerController playerCtrl = FindObjectOfType<OVRPlayerController>();
        if (playerCtrl != null)
        {
            playerCtrl.enabled = false; 
            
            Rigidbody rb = playerCtrl.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true; 
        }

        CharacterController charCtrl = FindObjectOfType<CharacterController>();
        if (charCtrl != null) charCtrl.enabled = false;

        StartCoroutine(EndingRoutine(endingType));
    }

    IEnumerator EndingRoutine(int endingType)
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            yield return new WaitWhile(() => audioSource.isPlaying);
            yield return new WaitForSeconds(0.5f);
        }

        if (fadeOverlay == null)
        {
            GameObject fadeObj = GameObject.Find("FadeOverlay");
            if (fadeObj != null) fadeOverlay = fadeObj.GetComponent<Image>();
        }

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

        GameObject globalPlayer = GameObject.Find("[Global_Player]");
        if (globalPlayer != null)
        {
            globalPlayer.transform.position = new Vector3(0, 1000f, 0);
        }

        if (VRLaserToggle.instance != null)
        {
            VRLaserToggle.instance.SetLaserState(true);
        }

        GameObject activeEndingPanel = null;
        if (endingType == 1) activeEndingPanel = ending1Panel;
        else if (endingType == 2) activeEndingPanel = ending2Panel;
        else if (endingType == 3) activeEndingPanel = ending3Panel;

        if (activeEndingPanel != null)
        {
            if (activeEndingPanel.transform.parent != null)
                activeEndingPanel.transform.parent.gameObject.SetActive(true);

            GameObject camObj = GameObject.Find("CenterEyeAnchor");
            if (camObj != null)
            {
                Transform playerEyes = camObj.transform;
                activeEndingPanel.transform.position = playerEyes.position + playerEyes.forward * 1.8f;
                activeEndingPanel.transform.LookAt(playerEyes);
                activeEndingPanel.transform.Rotate(0, 180, 0); 
                activeEndingPanel.transform.localScale = new Vector3(1f, 1f, 1f);
            }

            activeEndingPanel.SetActive(true);

            // ==========================================
            // 【修正 2】：播放各結局專屬音檔 (依照條件正確歸位)
            // ==========================================
            if (endingType == 1 && sound_EndingGood != null)
            {
                audioSource.PlayOneShot(sound_EndingGood); // 結局1 (找齊4件)
            }
            else if (endingType == 2)
            {
                // 結局2 (Bad Ending 沒找齊)：觸發海浪+槍聲
                StartCoroutine(PlayEndingBadAudioRoutine());
            }
            else if (endingType == 3 && sound_EndingFinal != null)
            {
                // 結局3 (Final Ending 隱藏解鎖)
                audioSource.PlayOneShot(sound_EndingFinal);
            }

            Canvas panelCanvas = activeEndingPanel.GetComponentInParent<Canvas>();
            if (panelCanvas != null) panelCanvas.sortingOrder = 100;
            
            if (fadeOverlay != null && fadeOverlay.canvas != null)
                fadeOverlay.canvas.sortingOrder = -10;

            currentActiveEnding = activeEndingPanel;
            canTriggerCoreMessage = true; 
        }
    }

    // 結局2 (Bad Ending) 專屬播音排程：海浪聲 -> 延遲 -> 3聲槍響
    IEnumerator PlayEndingBadAudioRoutine()
    {
        // 1. 播放海浪聲 (無限循環)
        if (bgmSource != null && sound_EndingBad_Waves != null)
        {
            bgmSource.clip = sound_EndingBad_Waves;
            bgmSource.loop = true;
            bgmSource.volume = bgmNormalVolume; // 重置音量
            bgmSource.Play();
        }

        // 2. 延遲 3 秒鐘 (可依照畫面呈現感覺自行修改這個數字)
        yield return new WaitForSeconds(3f);

        // 3. 連續播放 3 次槍聲
        if (audioSource != null && sound_EndingBad_Gunshot != null)
        {
            for (int i = 0; i < 3; i++)
            {
                audioSource.PlayOneShot(sound_EndingBad_Gunshot);
                yield return new WaitForSeconds(1.5f); // 槍聲與槍聲之間的間隔時間
            }
        }
    }

    IEnumerator TransitionToCoreMessage()
    {
        if (currentActiveEnding != null)
        {
            CanvasGroup endingCG = currentActiveEnding.GetComponent<CanvasGroup>();
            if (endingCG == null) endingCG = currentActiveEnding.AddComponent<CanvasGroup>();

            float timer = 0f;
            while (timer < 1.5f)
            {
                timer += Time.unscaledDeltaTime;
                endingCG.alpha = Mathf.Lerp(1, 0, timer / 1.5f);
                yield return null;
            }
            currentActiveEnding.SetActive(false);
        }

        if (coreTextGroup != null) coreTextGroup.alpha = 0f;
        if (returnButtonGroup != null) returnButtonGroup.alpha = 0f;

        if (coreMessagePanel != null && currentActiveEnding != null)
        {
            GameObject camObj = GameObject.Find("CenterEyeAnchor");
            if (camObj != null)
            {
                Transform playerEyes = camObj.transform;
                coreMessagePanel.transform.position = playerEyes.position + playerEyes.forward * 2.5f;
                coreMessagePanel.transform.LookAt(playerEyes);
                coreMessagePanel.transform.Rotate(0, 180, 0); 
                coreMessagePanel.transform.localScale = new Vector3(1f, 1f, 1f);
            }
            coreMessagePanel.SetActive(true);

            Canvas coreCanvas = coreMessagePanel.GetComponentInParent<Canvas>();
            if (coreCanvas != null) coreCanvas.sortingOrder = 100;
        }

        if (coreTextGroup != null)
        {
            float timer = 0f;
            while (timer < 1.5f)
            {
                timer += Time.unscaledDeltaTime;
                coreTextGroup.alpha = Mathf.Lerp(0, 1, timer / 1.5f);
                yield return null;
            }
        }

        yield return new WaitForSecondsRealtime(2f);

        if (fadeOverlay != null)
        {
            fadeOverlay.raycastTarget = false; 
        }

        if (returnButtonGroup != null)
        {
            float timer = 0f;
            while (timer < 1.5f)
            {
                timer += Time.unscaledDeltaTime;
                returnButtonGroup.alpha = Mathf.Lerp(0, 1, timer / 1.5f);
                yield return null;
            }
            
            returnButtonGroup.interactable = true;
            returnButtonGroup.blocksRaycasts = true;
        }
    }

    public void ReturnToMainMenu()
    {
        collectedItems.Clear();
        isTimerRunning = false;
        isGameEnding = false;
        isHiddenTaskRevealed = false;
        timeRemaining = 1200f; 
        canTriggerCoreMessage = false;

        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(0, 0, 0, 1); 
            fadeOverlay.raycastTarget = true;
        }

        MainMenuController.hasPlayedSession = true; 

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        GameObject oldPlayer = GameObject.Find("[Global_Player]");
        if (oldPlayer != null)
        {
            oldPlayer.SetActive(false); 
            Destroy(oldPlayer);
        }

        GameObject oldContext = GameObject.Find("Global Context");
        if (oldContext != null)
        {
            oldContext.SetActive(false);
            Destroy(oldContext);
        }

        string[] allItemNames = new string[] { 
            "TransferForm_Whole_Board", "TransferForm_Classroom", 
            "book_chair", "book_locker", "TransferForm_Whole_bad" 
        };
        foreach (string itemName in allItemNames)
        {
            GameObject leftover = GameObject.Find(itemName);
            if (leftover != null) 
            {
                leftover.SetActive(false);
                Destroy(leftover);
            }
            
            GameObject leftoverClone = GameObject.Find(itemName + "(Clone)");
            if (leftoverClone != null) 
            {
                leftoverClone.SetActive(false);
                Destroy(leftoverClone);
            }
        }

        instance = null;
        gameObject.SetActive(false); 
        Destroy(gameObject);

        SceneManager.LoadScene("Init");
    }
}