using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorProximityPress : MonoBehaviour
{
    [Header("目標場景設定 (回程請填 whole)")]
    public string targetSceneName = "whole";

    [Header("提示文字物件 (把畫布或文字 GameObject 拉進來)")]
    public GameObject promptUI; 

    private bool canOpenDoor = false; 

    private void Start()
    {
        // 遊戲一開始，確保提示文字是「隱藏」的
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    // 當玩家走進透明方塊的範圍時
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
        {
            canOpenDoor = true;

            // 【新增】玩家靠近了，把提示文字打開顯示
            if (promptUI != null)
            {
                promptUI.SetActive(true);
            }
        }
    }

    // 當玩家離開門的範圍時
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
        {
            canOpenDoor = false;

            // 【新增】玩家走遠了，把提示文字隱藏起來
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (canOpenDoor)
        {
            if (OVRInput.GetDown(OVRInput.RawButton.A) || OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
            {
                SpawnManager.lastScene = SceneManager.GetActiveScene().name;
                Debug.Log($"[門控系統] 記錄離開場景：{SpawnManager.lastScene}，準備前往 {targetSceneName}！");
                SceneManager.LoadScene(targetSceneName);
            }
        }
    }
}