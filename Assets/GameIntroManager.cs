using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; 

public class GameIntroManager : MonoBehaviour
{
    [Header("UI 元件連結")]
    public Image fadeOverlay;         
    public TextMeshProUGUI subtitleText; 

    [Header("音效設定")]
    public AudioSource introAudioSource; 
    
    [Tooltip("Init 場景黑畫面時播放的第一段音檔")]
    public AudioClip audioClip1; 
    [Range(0f, 1f)] public float audio1Volume = 1f; 

    [Tooltip("進入 Whole 場景時播放的第二段音檔")]
    public AudioClip audioClip2; 
    [Range(0f, 1f)] public float audio2Volume = 1f; 

    [System.Serializable]
    public struct SubtitleLine
    {
        public string text;       
        public float startTime;   
        public float duration;    
    }

    [Header("字幕設定 (目前不打字請將 Size 設為 0)")]
    public SubtitleLine[] audio1Subtitles;
    public SubtitleLine[] audio2Subtitles;

    [Header("場景與轉場設定")]
    public string nextSceneName = "whole"; 
    public float fadeDuration = 1.5f;

    private bool shouldProceedToPhase2 = false;
    private OVRPlayerController playerController;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, 0);
        if (subtitleText != null) subtitleText.text = "";
    }

    public void StartGameIntro()
    {
        if (VRLaserToggle.instance != null)
        {
            VRLaserToggle.instance.SetLaserState(false);
        }
        StartCoroutine(Phase1Routine());
    }

    IEnumerator Phase1Routine()
    {
        // 1. 畫面變黑 (Fade Out)
        yield return StartCoroutine(Fade(0, 1));

        // 2. 播放第一段音檔與字幕
        if (introAudioSource != null && audioClip1 != null)
        {
            introAudioSource.clip = audioClip1;
            introAudioSource.volume = audio1Volume; 
            introAudioSource.Play();
            
            StartCoroutine(PlaySubtitles(audio1Subtitles));

            // 【已修復】：加入 Time.timeScale == 0f 防呆機制，防止選單開啟時跳關
            while (introAudioSource != null && (introAudioSource.isPlaying || Time.timeScale == 0f))
            {
                // 只有在遊戲「未暫停」時，才允許按板機跳過，防止在選單裡誤觸
                if (Time.timeScale > 0f)
                {
                    if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger) || OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
                    {
                        introAudioSource.Stop(); 
                        break; 
                    }
                }
                yield return null; 
            }
        }

        // 強制確保跳過後字幕立即清空
        if (subtitleText != null) subtitleText.text = "";

        // 3. 載入下一關
        shouldProceedToPhase2 = true;
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == nextSceneName && shouldProceedToPhase2)
        {
            shouldProceedToPhase2 = false;

            // 換場景後，嘗試抓取新場景中的畫布與字幕，避免失憶
            if (fadeOverlay == null)
            {
                GameObject fadeObj = GameObject.Find("FadeOverlay");
                if (fadeObj != null) fadeOverlay = fadeObj.GetComponent<Image>();
            }

            if (subtitleText == null)
            {
                GameObject subObj = GameObject.Find("SubtitleText");
                if (subObj != null) subtitleText = subObj.GetComponent<TextMeshProUGUI>();
            }

            // 確保新場景的黑幕是實心的，並清除任何殘留字體，接著才開始 Phase 2
            if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, 1);
            if (subtitleText != null) subtitleText.text = "";

            StartCoroutine(Phase2Routine());
        }
    }

    IEnumerator Phase2Routine()
    {
        // 1. 鎖定玩家移動
        playerController = FindObjectOfType<OVRPlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // 2. 播放第二段音檔與字幕
        if (introAudioSource != null && audioClip2 != null)
        {
            introAudioSource.clip = audioClip2;
            introAudioSource.volume = audio2Volume; 
            introAudioSource.Play();
            StartCoroutine(PlaySubtitles(audio2Subtitles));
        }

        // 3. 畫面變亮 (Fade In)
        StartCoroutine(Fade(1, 0));

        // 4. 等待音檔 2 播完 (確保強制聽完)
        // 【已修復】：同樣加入 Time.timeScale == 0f 防呆機制
        while (introAudioSource != null && (introAudioSource.isPlaying || Time.timeScale == 0f))
        {
            yield return null;
        }

        if (subtitleText != null) subtitleText.text = "";

        // 5. 解除玩家移動鎖定
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // 6. 流程結束，自我刪除
        Destroy(gameObject);
    }

    IEnumerator PlaySubtitles(SubtitleLine[] subtitles)
    {
        if (subtitles == null || subtitles.Length == 0 || subtitleText == null) yield break;

        float timer = 0f;
        int index = 0;
        bool isDisplaying = false;

        // 【已修復】：保護字幕總迴圈不被暫停中斷
        while (introAudioSource != null && (introAudioSource.isPlaying || Time.timeScale == 0f))
        {
            timer += Time.deltaTime; // 暫停時 deltaTime 會是 0，所以計時器自動完美凍結
            if (index < subtitles.Length && !isDisplaying)
            {
                if (timer >= subtitles[index].startTime)
                {
                    isDisplaying = true;
                    StartCoroutine(DisplaySingleSubtitle(subtitles[index], () => isDisplaying = false));
                    index++;
                }
            }
            yield return null;
        }
    }

    IEnumerator DisplaySingleSubtitle(SubtitleLine line, System.Action onComplete)
    {
        if (subtitleText != null) subtitleText.text = line.text;
        
        float timer = 0f;
        // 【已修復】：保護單句字幕不被暫停中斷
        while (timer < line.duration && introAudioSource != null && (introAudioSource.isPlaying || Time.timeScale == 0f))
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (subtitleText != null) subtitleText.text = "";
        onComplete?.Invoke();
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (fadeOverlay == null) yield break;
        float timer = 0f;
        Color c = fadeOverlay.color;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            fadeOverlay.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }
        fadeOverlay.color = new Color(c.r, c.g, c.b, endAlpha);
    }
}