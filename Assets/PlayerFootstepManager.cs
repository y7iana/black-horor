using UnityEngine;

public class PlayerFootstepManager : MonoBehaviour
{
    [Header("腳步聲元件")]
    public AudioSource footstepSource;
    public CharacterController characterController;

    [Header("速度與音調微調")]
    [Tooltip("輕推搖桿（走很慢）時的播放速度")]
    public float minPitch = 0.8f;
    [Tooltip("搖桿推到底（走最快）時的播放速度")]
    public float maxPitch = 1.4f;
    [Tooltip("玩家的最大移動速度（用來計算比例，VR預設通常是 2 到 3）")]
    public float maxWalkSpeed = 2.5f;

    void Start()
    {
        // 自動抓取身上的元件 (防呆)
        if (characterController == null)
            characterController = GetComponentInParent<CharacterController>();
            
        if (footstepSource == null)
            footstepSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // 如果沒有綁定好，就不執行，避免報錯
        if (footstepSource == null || characterController == null) return;

        // 1. 取得玩家「水平方向」的真實移動速度 (忽略 Y 軸跳躍或掉落)
        Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        // 2. 判斷玩家是否有真正在移動 (並且踩在地板上)
        // 設定 currentSpeed > 0.1f 是為了防止搖桿些微飄移產生雜音
        if (currentSpeed > 0.1f && characterController.isGrounded)
        {
            // 如果還沒播放，就開始播放
            if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }

            // 3. 【核心魔法】：根據真實移動速度，動態改變音檔的 Pitch (播放速度)
            // 計算目前速度佔最大速度的比例 (會得到一個 0.0 到 1.0 之間的數字)
            float speedRatio = Mathf.Clamp01(currentSpeed / maxWalkSpeed);
            
            // 根據比例，在 minPitch 和 maxPitch 之間滑順切換！
            footstepSource.pitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);
        }
        else
        {
            // 如果停下來、撞到牆壁卡住、或是在空中，就暫停播放
            if (footstepSource.isPlaying)
            {
                footstepSource.Pause();
            }
        }
    }
}