using UnityEngine;
using System.Collections; // 為了使用延遲功能，必須加入這一行

public class SpawnManager : MonoBehaviour
{
    public static string lastScene = "FirstTime"; 

    [Header("玩家主相機")]
    public GameObject cameraRig;

    [Header("4 個隱形出生點")]
    public Transform spawnSchoolGate;
    public Transform spawnHall;
    public Transform spawnBath;
    public Transform spawnSport;

    void Start()
    {
        if (cameraRig == null)
        {
            cameraRig = GameObject.Find("[BuildingBlock] Camera Rig");
        }

        // 判斷要去哪個點，但這次我們不馬上傳送，而是呼叫「延遲傳送程序」
        Transform target = spawnSchoolGate;
        if (lastScene == "hall") target = spawnHall;
        else if (lastScene == "bath") target = spawnBath;
        else if (lastScene == "sport(1)") target = spawnSport;

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