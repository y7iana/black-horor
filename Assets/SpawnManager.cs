using UnityEngine;
using System.Collections; // 為了使用延遲功能，必須加入這一行

public class SpawnManager : MonoBehaviour
{
    public static string lastScene = "FirstTime"; 

    [Header("玩家主相機")]
    public GameObject cameraRig;

    [Header("5 個隱形出生點")]
    public Transform spawnSchoolGate;
    public Transform spawnHall;
    public Transform spawnBath;
    public Transform spawnSport;
    public Transform spawnClassroom; // 教室的出生點

    void Start()
    {
        if (cameraRig == null)
        {
            cameraRig = GameObject.Find("[BuildingBlock] Camera Rig");
        }

        Transform target = spawnSchoolGate;
        
        // 【防呆機制】把紀錄的場景名字強制轉成全小寫，再來比對！
        string checkScene = lastScene.ToLower();
        
        if (checkScene == "hall") target = spawnHall;
        else if (checkScene == "bath") target = spawnBath;
        else if (checkScene == "sport(1)") target = spawnSport;
        else if (checkScene == "classroom") target = spawnClassroom; // 這裡統一全部用小寫比對

        // 啟動延遲傳送
        StartCoroutine(DelayedTeleport(target));
    }

    // 這就是延遲傳送的神奇魔法
    IEnumerator DelayedTeleport(Transform targetSpawnPoint)
    {
        // 1. 先等 0.1 秒，讓 Meta VR 頭盔把地板跟追蹤系統準備好！
        yield return new WaitForSeconds(0.1f);

        if (targetSpawnPoint != null && cameraRig != null)
        {
            // 2. 尋找並暫停玩家身上的所有物理外殼
            CharacterController cc = cameraRig.GetComponentInChildren<CharacterController>();
            Rigidbody rb = cameraRig.GetComponentInChildren<Rigidbody>();

            if (cc != null) cc.enabled = false;
            if (rb != null) 
            {
                rb.isKinematic = true; // 關閉物理重力
                rb.velocity = Vector3.zero; // 煞車，消除往下掉的速度
            }

            // 3. 瞬間移動
            cameraRig.transform.position = targetSpawnPoint.position;
            cameraRig.transform.rotation = targetSpawnPoint.rotation;

            // 4. 再等一個畫面偵，確保位置已經確實移動過去了
            yield return null;

            // 5. 把物理外殼重新打開，讓玩家穩穩踩在地板上
            if (cc != null) cc.enabled = true;
            if (rb != null) rb.isKinematic = false;

            Debug.Log($"[出生點系統] 成功延遲傳送到了：{targetSpawnPoint.name}！");
        }
    }
}