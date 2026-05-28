using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{
    public void ClickStartGame()
    {
        // 1. 設定第一次進入遊戲的傳送點 ID
        SceneBridge.nextSpawnID = "Gate";

        // 2. 載入主場景
        SceneManager.LoadScene("whole");
    }
}