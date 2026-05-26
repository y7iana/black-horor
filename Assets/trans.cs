using UnityEngine;
using UnityEngine.SceneManagement; 

/**
 * 專門給 VR 互動按鈕或門使用的換場景腳本
 */
public class DoorSceneTransition : MonoBehaviour
{
    [Header("目標場景設定")]
    [Tooltip("在 Inspector 中可以隨時改成你要去的場景名稱")]
    // 這裡我幫你把預設值改成 hall 了！
    public string targetSceneName = "hall"; 

    public void OpenDoorAndLoadScene()
    {
        Debug.Log($"[開門換場景] 玩家準備從 {SceneManager.GetActiveScene().name} 前往：{targetSceneName}");
        SceneManager.LoadScene(targetSceneName);
    }
}