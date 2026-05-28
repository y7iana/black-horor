using UnityEngine;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    [Header("出生點 ID (必須與傳送門設定的 ID 一模一樣)")]
    public string spawnPointID;

    IEnumerator Start()
    {
        string target = SceneBridge.nextSpawnID;

        if (string.IsNullOrEmpty(target))
        {
            target = "Gate";
        }

        // 判斷是不是找我，不是就直接退出
        if (spawnPointID != target) 
        {
            yield break;
        }

        // ==========================================
        // 【核心修正】：先等待 0.2 秒！
        // 讓場景中其他的 PlayerSpawner 都有時間讀取到正確的 target，
        // 並且執行 yield break 乖乖退場。
        // ==========================================
        yield return new WaitForSeconds(0.2f);
        
        // 等其他出生點都罷工後，我們再安全地清空橋樑
        SceneBridge.nextSpawnID = "";

        GameObject playerRoot = GameObject.Find("[Global_Player]");
        
        if (playerRoot != null)
        {
            CharacterController cc = playerRoot.GetComponent<CharacterController>();
            
            Transform centerEye = null;
            OVRCameraRig rig = playerRoot.GetComponentInChildren<OVRCameraRig>();
            if (rig != null) {
                centerEye = rig.transform.Find("TrackingSpace/CenterEyeAnchor");
            }

            if (cc != null) cc.enabled = false;

            if (centerEye != null)
            {
                // 1. 先處理轉向
                float currentHeadAngle = centerEye.eulerAngles.y;
                float targetAngle = this.transform.eulerAngles.y;
                playerRoot.transform.Rotate(0, targetAngle - currentHeadAngle, 0);

                // 2. 再處理位置
                Vector3 offset = centerEye.position - playerRoot.transform.position;
                offset.y = 0; 
                
                playerRoot.transform.position = this.transform.position - offset;
            }
            else
            {
                // 如果抓不到頭盔的備用方案
                playerRoot.transform.position = this.transform.position;
                playerRoot.transform.rotation = this.transform.rotation;
            }

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            if (cc != null) cc.enabled = true;
            
            Debug.Log($"【精準傳送】成功降落並轉向於 {spawnPointID}！");
        }
        else
        {
            Debug.LogError("【傳送失敗】找不到 [Global_Player]！");
        }
    }
}