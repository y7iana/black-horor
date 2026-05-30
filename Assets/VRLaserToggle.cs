using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class VRLaserToggle : MonoBehaviour
{
    public static VRLaserToggle instance; 
    
    [Header("UI 點擊大腦")]
    public EventSystem uiEventSystem; 
    private OVRInputModule ovrInput; 

    [Header("請把左右手拖進來")]
    public Transform rightHand;
    public Transform leftHand;

    [Header("射線設定")]
    [Tooltip("射線的最遠偵測距離")]
    public float maxDistance = 20f; 

    private LineRenderer rightLaser;
    private LineRenderer leftLaser;
    
    private GameObject rightReticle;
    private GameObject leftReticle;

    void Awake()
    {
        instance = this;
        
        if (rightHand != null) 
        {
            rightLaser = SetupLaser(rightHand.gameObject);
            rightReticle = CreateReticle();
        }
        if (leftHand != null) 
        {
            leftLaser = SetupLaser(leftHand.gameObject);
            leftReticle = CreateReticle();
        }

        if (uiEventSystem != null)
        {
            ovrInput = uiEventSystem.GetComponent<OVRInputModule>();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    LineRenderer SetupLaser(GameObject handObj)
    {
        LineRenderer laser = handObj.AddComponent<LineRenderer>();
        laser.startWidth = 0.005f;
        laser.endWidth = 0.005f;
        laser.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        laser.startColor = Color.cyan; 
        return laser;
    }

    // ==========================================
    // 【還原】：變回原本可愛的立體小圓球！
    // ==========================================
    GameObject CreateReticle()
    {
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(dot.GetComponent<Collider>()); // 刪除碰撞體以免擋住射線
        
        // 維持 X, Y, Z 等比例的立體圓球
        dot.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f); 
        
        Renderer rend = dot.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Unlit/Color"));
        rend.material.color = Color.cyan;
        
        dot.SetActive(false); 
        return dot;
    }

    void Update()
    {
        if (ovrInput != null)
        {
            if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger)) ovrInput.rayTransform = leftHand;
            else if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger)) ovrInput.rayTransform = rightHand;
        }

        UpdateLaser(rightHand, rightLaser, rightReticle);
        UpdateLaser(leftHand, leftLaser, leftReticle);
    }

    void UpdateLaser(Transform hand, LineRenderer laser, GameObject reticle)
    {
        if (hand == null || laser == null || !laser.enabled) 
        {
            if (reticle != null) reticle.SetActive(false);
            return;
        }

        laser.SetPosition(0, hand.position);

        if (Physics.Raycast(hand.position, hand.forward, out RaycastHit hit, maxDistance))
        {
            laser.SetPosition(1, hit.point);
            laser.endColor = Color.cyan; 
            
            if (reticle != null)
            {
                reticle.SetActive(true);
                // 【關鍵防覆蓋】：讓圓球往射線反方向退後 1.5 公分，它就會穩穩浮在按鈕表面上！
                reticle.transform.position = hit.point - hand.forward * 0.015f; 
            }
        }
        else
        {
            laser.SetPosition(1, hand.position + hand.forward * maxDistance);
            
            // 【關鍵修改】：將此處的顏色從透明 (new Color(0, 1, 1, 0)) 改為 Color.cyan
            // 確保雷射在沒有指到任何東西的黑畫面中，依然保持清晰可見的青色射線。
            laser.endColor = Color.cyan; 
            
            if (reticle != null) reticle.SetActive(false);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Init") SetLaserState(true);
        else SetLaserState(false);
    }

    public void SetLaserState(bool isOn)
    {
        if (rightLaser != null) rightLaser.enabled = isOn;
        if (leftLaser != null) leftLaser.enabled = isOn;
        if (uiEventSystem != null) uiEventSystem.enabled = isOn;
        
        if (!isOn)
        {
            if (rightReticle != null) rightReticle.SetActive(false);
            if (leftReticle != null) leftReticle.SetActive(false);
        }
    }
}