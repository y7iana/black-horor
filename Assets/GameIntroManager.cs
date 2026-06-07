using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video; // 負責控制影片播放的模組
using System.Collections;
using TMPro; 

public class GameIntroManager : MonoBehaviour
{
    [Header("UI 元件連結")]
    public Image fadeOverlay;         
    public TextMeshProUGUI subtitleText; 

    // ==========================================
    // 【新增】：VR360 影片與天空盒設定
    // ==========================================
    [Header("VR360 開場影片設定")]
    [Tooltip("請把場景中的 Video Player 拖進來")]
    public VideoPlayer vr360Video; 
    
    [Tooltip("請把你做好的 VR360_Mat 材質球拖進來")]
    public Material vr360SkyboxMaterial; 
    
    // 用來記憶原本的森林天空，方便看完影片後復原
    private Material originalSkybox; 

    [Header("音效設定")]
    public AudioSource introAudioSource; 
    
    [Tooltip("進入 Whole 場景時播放的音檔")]
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
        // 1. 畫面先變全黑 (Fade Out)，遮住森林主選單
        yield return StartCoroutine(Fade(0, 1));

        // ==========================================
        // 【魔術時刻】：在全黑的時候，偷偷把天空換成 VR360 影片！
        // ==========================================
        if (vr360SkyboxMaterial != null)
        {
            originalSkybox = RenderSettings.skybox; // 先把森林天空記在腦海裡
            RenderSettings.skybox = vr360SkyboxMaterial; // 換上 VR360 天空
        }

        // 2. 準備並播放 VR360 影片
        if (vr360Video != null)
        {
            // 讓影片先在黑畫面背後準備好，防止卡頓
            vr360Video.Prepare();
            while (!vr360Video.isPrepared) 
            {
                yield return null;
            }

            // 準備好後開始播放
            vr360Video.Play();

            // 3. 畫面變透明 (Fade In)，玩家看見 360 影片
            yield return StartCoroutine(Fade(1, 0));

            // 給予 0.5 秒緩衝時間，確保影片狀態切換完成
            yield return new WaitForSeconds(0.5f);

            // 4. 等待影片播完，或是玩家按板機跳過
            while (vr360Video.isPlaying || Time.timeScale == 0f)
            {
                if (Time.timeScale > 0f)
                {
                    if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger) || OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
                    {
                        vr360Video.Stop(); 
                        break; 
                    }
                }
                yield return null; 
            }

            // 5. 影片結束後，畫面再次變全黑 (Fade Out)，準備切換場景
            yield return StartCoroutine(Fade(0, 1));
        }

        // ==========================================
        // 復原動作：把森林天空換回來，確保不影響後續的關卡
        // ==========================================
        if (originalSkybox != null)
        {
            RenderSettings.skybox = originalSkybox;
        }

        // 強制確保跳過後字幕立即清空
        if (subtitleText != null) subtitleText.text = "";

        // 6. 載入下一關 (whole 場景)
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

        while (introAudioSource != null && (introAudioSource.isPlaying || Time.timeScale == 0f))
        {
            timer += Time.deltaTime; 
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