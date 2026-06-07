using UnityEngine;
using UnityEngine.UI;
using System.Collections; 

public class MainMenuController : MonoBehaviour
{
    [Header("選單底圖設定")]
    public Image mainMenuBackground; 
    public Sprite firstTimeImage;    
    public Sprite returnedImage;     

    [Header("主選單音樂設定")]
    public AudioSource bgmSource;     
    public float fadeDuration = 1.5f; 

    // ==========================================
    // 【全新升級】：暫存記憶系統 (App 關閉重開後自動失效)
    // ==========================================
    public static bool hasPlayedSession = false; 

    void Start()
    {
        // 解決問題 3：一進入主選單，手動觸發一次黑畫面淡入動畫
        StartCoroutine(FadeInFromBlack());

        // 確保音樂一開始是正常的音量
        if (bgmSource != null) bgmSource.volume = 1f;

        // ==========================================
        // 讀取暫存記憶
        // ==========================================
        if (hasPlayedSession == true)
        {
            // 同一次開啟 App 的情況下，玩完退回主選單，顯示反轉結局
            if (mainMenuBackground != null && returnedImage != null)
                mainMenuBackground.sprite = returnedImage;
        }
        else
        {
            // 第一次打開 App，或是隔天重新開啟 App，顯示拯救社團
            if (mainMenuBackground != null && firstTimeImage != null)
                mainMenuBackground.sprite = firstTimeImage;
        }
    }

    public void FadeOutBGMOnly()
    {
        if (bgmSource != null)
            StartCoroutine(FadeOutCoroutine());
    }

    IEnumerator FadeOutCoroutine()
    {
        float startVolume = bgmSource.volume;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; 
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }
        bgmSource.volume = 0f;
    }

    // 解決問題 3：讓主選單自己處理開場的黑畫面淡入
    IEnumerator FadeInFromBlack()
    {
        GameObject fadeObj = GameObject.Find("FadeOverlay");
        if (fadeObj != null)
        {
            Image fadeImg = fadeObj.GetComponent<Image>();
            if (fadeImg != null)
            {
                fadeImg.color = new Color(0, 0, 0, 1); // 先全黑
                float timer = 0f;
                while (timer < 1.5f)
                {
                    timer += Time.unscaledDeltaTime;
                    fadeImg.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, timer / 1.5f));
                    yield return null;
                }
                fadeImg.raycastTarget = false;
            }
        }
    }
}