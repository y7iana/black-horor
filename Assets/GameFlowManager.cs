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

    [Header("收集語音設定")]
    public AudioSource audioSource;
    
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
        
        if (coreMessagePanel != null) coreMessagePanel.SetActive(false); 
        
        if (timerText != null) timerText.gameObject.SetActive(false);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (autoStartTimer) StartTimer(); 
    }

    void Update()
    {
        if (isTimerRunning && !isGameEnding)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                isTimerRunning = false;

                if (HasCollectedAllRequiredItems())
                    TriggerEnding(1); 
                else
                    TriggerEnding(2); 
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
        
        if (HasCollectedAllRequiredItems() && collectedItems.Contains(hiddenEndingItem))
        {
            TriggerEnding(3); 
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

    private void TriggerEnding(int endingType)
    {
        isTimerRunning = false; 
        isGameEnding = true; 
        StartCoroutine(EndingRoutine(endingType));
    }

    IEnumerator EndingRoutine(int endingType)
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            yield return new WaitWhile(() => audioSource.isPlaying);
            yield return new WaitForSeconds(0.5f);
        }

        OVRPlayerController playerCtrl = FindObjectOfType<OVRPlayerController>();
        if (playerCtrl != null) playerCtrl.enabled = false;

        CharacterController charCtrl = FindObjectOfType<CharacterController>();
        if (charCtrl != null) charCtrl.enabled = false;

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

            Canvas panelCanvas = activeEndingPanel.GetComponentInParent<Canvas>();
            if (panelCanvas != null) panelCanvas.sortingOrder = 100;
            
            if (fadeOverlay != null && fadeOverlay.canvas != null)
                fadeOverlay.canvas.sortingOrder = -10;

            currentActiveEnding = activeEndingPanel;
            canTriggerCoreMessage = true; 
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
        // ==========================================
        // 【修正 1】：離開前先隱藏計時器，避免它殘留在主選單
        // ==========================================
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(0, 0, 0, 0); 
            fadeOverlay.raycastTarget = false;
        }

        OVRPlayerController playerCtrl = FindObjectOfType<OVRPlayerController>();
        if (playerCtrl != null) playerCtrl.enabled = true;

        CharacterController charCtrl = FindObjectOfType<CharacterController>();
        if (charCtrl != null) charCtrl.enabled = true;

        // ==========================================
        // 【修正 2】：把玩家從 1000m 高空移回正常地板 (0,0,0)
        // ==========================================
        GameObject globalPlayer = GameObject.Find("[Global_Player]");
        if (globalPlayer != null)
        {
            globalPlayer.transform.position = Vector3.zero;
            // 消除旋轉，確保一回到主場景時視角是正前方
            globalPlayer.transform.rotation = Quaternion.identity; 
        }

        Destroy(gameObject);
        SceneManager.LoadScene("Init");
    }
}