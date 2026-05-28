using UnityEngine;



[RequireComponent(typeof(CharacterController))]

public class VRSmoothMove : MonoBehaviour

{

    [Header("移動設定")]

    public float moveSpeed = 3.0f; 

    public float turnSpeed = 45.0f; 

    public float gravity = -9.81f;  



    [Header("請把 CenterEyeAnchor 拖進來")]

    public Transform centerEye;     



    private CharacterController cc;

    private float verticalVelocity = 0f;



    void Start()

    {

        cc = GetComponent<CharacterController>();

    }



    void Update()

    {

        if (!cc.enabled) 

        {

            verticalVelocity = 0f; 

            return;

        }



        // ==========================================

        // 【虛空撈回機制】：防止無限墜落

        // ==========================================

        // 如果玩家掉到 Y 座標小於 -5 (代表已經穿模掉下去了)

        if (transform.position.y < -5f)

        {

            Debug.LogWarning("【防護系統啟動】偵測到玩家掉入虛空，強制拉回半空中！");

            cc.enabled = false;

            // 把玩家強制拉回 X 和 Z 的原位，但高度重置為 3 公尺的半空中

            transform.position = new Vector3(transform.position.x, 3f, transform.position.z);

            verticalVelocity = 0f;

            cc.enabled = true;

            return; 

        }



        // 追蹤頭部位置 (維持防護罩對齊頭部)

        Vector3 localHeadPos = transform.InverseTransformPoint(centerEye.position);

        cc.center = new Vector3(localHeadPos.x, cc.center.y, localHeadPos.z);



        // 處理轉向

        Vector2 rightStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        if (Mathf.Abs(rightStick.x) > 0.1f) 

        {

            float angle = rightStick.x * turnSpeed * Time.deltaTime;

            transform.RotateAround(centerEye.position, Vector3.up, angle);

        }



        // 處理移動

        Vector2 leftStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        Vector3 forward = centerEye.forward;

        Vector3 right = centerEye.right;

        

        forward.y = 0;

        right.y = 0;

        forward.Normalize();

        right.Normalize();



        Vector3 moveDir = forward * leftStick.y + right * leftStick.x;



        // 處理重力

        if (cc.isGrounded)

        {

            // 給予微小下壓力量，確保不會在斜坡上亂跳

            verticalVelocity = -2f; 

        }

        else

        {

            verticalVelocity += gravity * Time.deltaTime;

        }



        Vector3 finalMove = moveDir * moveSpeed;

        finalMove.y = verticalVelocity;



        cc.Move(finalMove * Time.deltaTime);

    }

} 

