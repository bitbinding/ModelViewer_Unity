using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScaleButton : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("按下后缩放系数")]
    [Range(0.9f,1.1f)]
    public float pressedScale=1.05f;
    [Header("播放持续时间(毫秒)")]
    [Range(100,500)]
    public int interval=200;

    private int frame=0;
    private int framePrev=0;
    private int frameCount=1;
    private float initialScale=1f;
    void Awake()
    {
        initialScale=gameObject.transform.localScale.y;
        frameCount=Mathf.Max(1,interval*60/1000);
        frame=0;
        Button btn=gameObject.GetComponent<Button>()!=null?gameObject.GetComponent<Button>():gameObject.AddComponent<Button>();
        btn.transition=Selectable.Transition.None;
    }

    // Update is called once per frame
    void Update()
    {
        bool isCurrent=UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject==gameObject;
        bool pressing=false;
        if(isCurrent && Input.GetMouseButton(0) && EventSystem.current.IsPointerOverGameObject()){
            pressing=true;
        }
        for(int i = Input.touchCount-1; i>=0; i--) {
            if(isCurrent && EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId)) {
                pressing=true;
                break;
            }
        }
        framePrev=frame;
        if(pressing) {
            if(frame<frameCount) {
                frame++;
            }
        } else {
            if(frame>0) {
                frame--;
            }
        }
        if(frame!=framePrev) {
            float framePos=frame/(float)frameCount;
            if(framePos<=0.5) {
                framePos=2f*framePos*framePos;
            } else {
                framePos=1f-2f*(1f-framePos)*(1f-framePos);
            }
            float scale=initialScale+framePos*(pressedScale-initialScale);
            gameObject.transform.localScale=new Vector3(scale,scale,scale);
        }
    }
}
