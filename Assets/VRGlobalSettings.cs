using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VRGlobalSettings : MonoBehaviour
{
    public static VRGlobalSettings instance;

    [Header("設定介面物件")]
    public GameObject settingsUI;         // 放入你的 SettingsPanel
    public Transform centerEyeCamera;     // 玩家的眼睛
    
    [Header("主選單連結 (僅 Init 場景需要)")]
    public GameObject mainMenuPanel;      // 放入你的 MainMenuPanel

    private bool isSettingsOpen = false;
    private bool wasMainMenuActive = false; // 新增：精準記憶打開前主選單是否亮著

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

    void Start()
    {
        if (settingsUI != null) settingsUI.SetActive(false);
    }

    void Update()
    {
        // 任何時候按右手 A 鍵都能切換選單
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            if (isSettingsOpen)
                CloseSettings();
            else
                OpenSettings(false); 
        }
    }

    // 當從主選單按鈕點擊時
    public void OpenSettingsFromMainMenu()
    {
        OpenSettings(true);
    }

    private void OpenSettings(bool fromMainMenu)
    {
        isSettingsOpen = true;

        // 【修復問題 3】：自動尋找並檢查主選單按鈕面板是否正亮著
        if (mainMenuPanel == null)
        {
            mainMenuPanel = GameObject.Find("MainMenuPanel");
        }

        // 不管是按按鈕還是按 A 鍵，只要主選單當時存在且開著，就記錄並隱藏它
        if (mainMenuPanel != null && mainMenuPanel.activeSelf)
        {
            wasMainMenuActive = true;
            mainMenuPanel.SetActive(false);
        }
        else
        {
            if (fromMainMenu) wasMainMenuActive = true;
            else wasMainMenuActive = false;
        }

        // 顯示設定選單
        if (settingsUI != null) settingsUI.SetActive(true);

        // 調整位置到眼前 1 公尺
        if (centerEyeCamera == null)
        {
            GameObject cam = GameObject.Find("CenterEyeAnchor");
            if (cam != null) centerEyeCamera = cam.transform;
        }

        if (centerEyeCamera != null)
        {
            settingsUI.transform.position = centerEyeCamera.position + centerEyeCamera.forward * 1.0f;
            settingsUI.transform.LookAt(centerEyeCamera);
            settingsUI.transform.Rotate(0, 180, 0); 
        }

        // 打開選單時，一律強制喚醒雷射筆以便操作
        if (VRLaserToggle.instance != null)
        {
            VRLaserToggle.instance.SetLaserState(true);
        }

        Time.timeScale = 0f;
        AudioListener.pause = true; 
    }

    public void CloseSettings()
    {
        isSettingsOpen = false;

        if (settingsUI != null) settingsUI.SetActive(false);

        // 解除時停
        Time.timeScale = 1f;
        AudioListener.pause = false;

        string currentScene = SceneManager.GetActiveScene().name;

        // 【修復問題 1 & 4】：智慧返回邏輯
        // 只有在 init 場景且當初是從主選單介面點開時，才退回主選單並保持雷射
        if (currentScene == "Init" && wasMainMenuActive)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            
            if (VRLaserToggle.instance != null)
            {
                VRLaserToggle.instance.SetLaserState(true); // 保持雷射亮著，讓玩家可以點「開始遊戲」
            }
        }
        else
        {
            // 其餘任何時候（黑畫面放音檔1、whole、bath 等等），關閉選單就是回到遊戲，並關閉雷射
            if (VRLaserToggle.instance != null)
            {
                VRLaserToggle.instance.SetLaserState(false); 
            }
        }

        wasMainMenuActive = false; // 重置狀態
    }
    
    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void ExitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}