using UnityEngine;
using System.Collections; // 【新增】：使用協程必須加入這行

public class HiddenItemUnlocker : MonoBehaviour
{
    private MeshRenderer mesh;
    private Collider col;
    
    // 用來防止重複觸發的鎖
    private bool isUnlocking = false;

    [Header("隱藏任務提示音效 (例如：突兀的廣播聲、雜訊)")]
    public AudioSource spookyAudioSource;

    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();
        
        // 遊戲一開始，強制把自己隱形、且無法被雷射抓取
        if (mesh != null) mesh.enabled = false;
        if (col != null) col.enabled = false;
    }

    void Update()
    {
        // 如果還沒開始解鎖程序，而且大腦說前四個核心道具找齊了
        if (!isUnlocking && GameFlowManager.instance != null && GameFlowManager.instance.HasCollectedAllRequiredItems())
        {
            isUnlocking = true; // 上鎖！避免 Update 每秒狂呼叫
            
            // 開始執行「等待與解鎖」的排程
            StartCoroutine(UnlockRoutine());
        }
    }

    // ==========================================
    // 【關鍵新增】：控制時間與節奏的協程
    // ==========================================
    IEnumerator UnlockRoutine()
    {
        // 1. 等待大腦的「收集故事語音」播放完畢
        if (GameFlowManager.instance.audioSource != null)
        {
            // WaitWhile 會一直卡在這裡，直到大腦的聲音停止播放 (isPlaying 變成 false)
            yield return new WaitWhile(() => GameFlowManager.instance.audioSource.isPlaying);
        }

        // 2. 故事講完了，製造 3 秒鐘令人不安的死寂
        yield return new WaitForSeconds(3f);

        // 3. 3 秒過後，隱藏的壞轉學單現出原形！
        if (mesh != null) mesh.enabled = true;
        if (col != null) col.enabled = true;
        
        // 4. 同時播放隱藏任務的提示音檔，嚇玩家一跳！
        if (spookyAudioSource != null)
        {
            spookyAudioSource.Play();
        }
        
        // 任務完成，關閉這個偵測腳本以節省效能
        this.enabled = false;
    }
}