using UnityEngine;

public class VRItem : MonoBehaviour
{
    private VRBackpack backpack;
    private bool isBeingHeld = false; 
    private Rigidbody rb; 

    void Start()
    {
        backpack = FindObjectOfType<VRBackpack>();
        rb = GetComponent<Rigidbody>(); 
    }

    void Update()
    {
        // 【已修正】：現在只會偵測左手 X 鍵 (RawButton.X)
        if (isBeingHeld && OVRInput.GetDown(OVRInput.RawButton.X))
        {
            if (backpack != null)
            {
                backpack.AddItem(this.gameObject); 
                isBeingHeld = false; 
            }
        }
    }

    // 當被手抓住的瞬間
    public void OnGrabStart() 
    { 
        isBeingHeld = true; 
        if (rb != null) rb.isKinematic = false; 
    }
    
    // 當被手放開的瞬間
    public void OnGrabEnd() 
    { 
        isBeingHeld = false; 
        
        // 【打敗 Meta 的防護機制】：
        // 在放手時，強制告訴系統「不准變回暫停狀態，給我掉下去！」
        if (rb != null)
        {
            rb.isKinematic = false; 
        }
    }
}