using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSingleton : MonoBehaviour
{
    private static PlayerSingleton instance;
    private CharacterController cc; // 記錄玩家的物理大腦

    void Awake()
    {
        // 1. 滅鬼程序
        if (instance != null)
        {
            gameObject.SetActive(false);
            Destroy(this.gameObject);
            return; 
        }

        // 2. 註冊真玩家
        instance = this;
        DontDestroyOnLoad(this.transform.root.gameObject);
        
        // 抓取身上的 CharacterController (不管它掛在父層還是子層)
        cc = GetComponentInChildren<CharacterController>(true);
        
        // 監聽場景切換事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 當任何場景載入完成時，這段程式碼會自動執行
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (cc == null) cc = GetComponentInChildren<CharacterController>(true);
        
        if (cc != null)
        {
            // 【核心修正】：無論載入哪個場景，第一時間先「強制關閉」物理與重力！
            // 防止在 PlayerSpawner 找座標的那 0.2 秒內，玩家在虛空中往下掉。
            cc.enabled = false;
            
            if (scene.name == "Init")
            {
                Debug.Log("[重力系統] 位於 Init 場景，重力持續關閉。");
            }
            else
            {
                // 開啟重力的工作，現在全權交給 PlayerSpawner 在傳送對位完成後執行
                Debug.Log($"[重力系統] 抵達 {scene.name}，等待 PlayerSpawner 放置並重啟重力...");
            }
        }
    }
}