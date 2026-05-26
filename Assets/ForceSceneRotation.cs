using UnityEngine;
using System.Collections;

public class ForceSceneRotation : MonoBehaviour
{
    [Header("一進入場景想要面對的角度 (度數)")]
    [Tooltip("可以填寫 0, 90, 180, 270 或 -90 等，根據測試調整")]
    public float targetYRotation = 0f;

    void Start()
    {
        // 呼叫延遲轉向程序
        StartCoroutine(DelayedRotate());
    }

    IEnumerator DelayedRotate()
    {
        // 核心魔法：等 0.1 秒，避開 Meta VR 剛載入時的硬體覆蓋
        yield return new WaitForSeconds(0.1f);

        // 強制將自身（Camera Rig）的 Y 軸旋轉設定到目標角度
        transform.rotation = Quaternion.Euler(0, targetYRotation, 0);
        
        Debug.Log($"[視角總管] 已成功將玩家轉向至：{targetYRotation} 度");
    }
}