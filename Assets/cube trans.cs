using UnityEngine;
using UnityEngine.SceneManagement; 

public class Newscenetrans : MonoBehaviour
{
    public string targetSceneName = "hall"; 
    public string playerTag = "Player";
    public string cameraTag = "MainCamera";

    private void OnTriggerEnter(Collider other)
    {
        bool isPlayer = other.CompareTag(playerTag);
        bool isCamera = other.CompareTag(cameraTag);
        bool isParentPlayer = other.transform.parent != null && other.transform.parent.CompareTag(playerTag);

        if (isPlayer || isCamera || isParentPlayer)
        {
            // 【新增的神奇魔法】自動抓取「現在這個場景」的名字，並寫進 SpawnManager 的備忘錄裡
            SpawnManager.lastScene = SceneManager.GetActiveScene().name;

            // 接著再執行原本的換場景動作
            SceneManager.LoadScene(targetSceneName);
        }
    }
}