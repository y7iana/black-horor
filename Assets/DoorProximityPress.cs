using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorProximityPress : MonoBehaviour
{
    [Header("目標場景名稱")]
    public string targetSceneName = "whole";

    [Header("目標出生點 ID (必須與目標場景中 PlayerSpawner 的 ID 一致)")]
    public string targetSpawnID = "Gate"; 

    [Header("提示文字物件")]
    public GameObject promptUI; 

    private bool canOpenDoor = false; 

    private void Start()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 確保偵測到玩家
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
        {
            canOpenDoor = true;
            if (promptUI != null) promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
        {
            canOpenDoor = false;
            if (promptUI != null) promptUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (canOpenDoor)
        {
            // 監聽按鍵輸入
            if (OVRInput.GetDown(OVRInput.RawButton.A) || OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
            {
                // 【關鍵修正】：使用 SceneBridge 記憶體橋樑傳遞資料
                // 這比 PlayerPrefs 更快、更穩定，完全不會有讀寫延遲
                SceneBridge.nextSpawnID = targetSpawnID;

                Debug.Log($"[門控系統] 準備前往 {targetSceneName}，目的地 ID: {targetSpawnID}");
                
                // 載入場景
                SceneManager.LoadScene(targetSceneName);
            }
        }
    }
}