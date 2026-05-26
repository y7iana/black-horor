using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorProximityPress : MonoBehaviour
{
    [Header("目標場景設定 (回程請填 whole)")]
    public string targetSceneName = "whole";

    // 這個開關用來紀錄「玩家現在是不是站在門前面」
    private bool canOpenDoor = false; 

    // 當玩家走進透明方塊的範圍時，把開關打開
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
        {
            canOpenDoor = true;
        }
    }

    // 當玩家離開門的範圍時，把開關關掉，避免在遠處誤觸
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
        {
            canOpenDoor = false;
        }
    }

    // 遊戲隨時都在檢查：玩家是否在門邊？且是否按下了按鈕？
    private void Update()
    {
        if (canOpenDoor)
        {
            // OVRInput.RawButton.A 代表右手的 A 鍵
            // OVRInput.RawButton.RIndexTrigger 代表右手的食指板機鍵
            if (OVRInput.GetDown(OVRInput.RawButton.A) || OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
            {
                // 【新增的神奇魔法】按下去的瞬間，把當前場景的名字寫進備忘錄！
                SpawnManager.lastScene = SceneManager.GetActiveScene().name;

                Debug.Log($"[門控系統] 記錄離開場景：{SpawnManager.lastScene}，準備前往 {targetSceneName}！");
                
                // 執行換場景
                SceneManager.LoadScene(targetSceneName);
            }
        }
    }
}