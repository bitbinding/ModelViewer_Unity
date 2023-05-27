using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Main : MonoBehaviour {

	// Use this for initialization

    Model3d model3d=null;
    GameObject model=null;

    float rx=0.0f;
    float ry=0.0f;
    ListLabels listLabels;
    bool roaming=false;

    float scaleLoaded=1f;
    Vector3 posLoaded=new Vector3(0,0,0);

    Vector3 cameraInitialPosition;
    Vector3 cameraInitialEulerAngles;
    float cameraInitialFieldOfView;
    
    Camera camera=null;

    float scaleOri=1f;
    Vector4 scalePosOri=new Vector4(0,0,1,1);
    bool hasScaleOri=false;
    bool useJoyStick=false;
    GameObject canvasJoyStick=null;
    Light light=null;

    bool receiveFunctionModelShadows=true;
    bool receiveExternalModelShadows=true;
    bool receiveExternalSceneShadows=true;

    public const float joyStickRMax=80f;
    public const float joyStickVMax=0.2f;
    public const float keyboardVMax=0.025f;
    public const float liftButtonV=0.015f;
    public const float sceneScale=0.01f;

    public const float mobileScanNear=2f;
    public const float mobileScanFar=30f;

    public const float mobilePanoNear=0.5f;
    public const float mobilePanoFar=400f;

    bool pressedJoyStick=false;
    Vector2 pressedJoyStickPos=new Vector2(0,0);
    int pressedJoyStickTouchId=-1;

    bool pressedAscendButton=false;
    bool pressedDescendButton=false;
    int pressedLiftTouchId=-1;
    private float lightOriIntensity = 1f;

    private Light cameraLight = null;
    Vector3 cameraToLight=new Vector3();

    public Material transparentMaterial;

    private void Awake() {
        transform.Find("PlotButton").GetComponent<Button>().onClick.AddListener(onPlot);
        transform.Find("LoadButton").GetComponent<Button>().onClick.AddListener(onModel);
        listLabels=transform.Find("BrowsePanel").GetComponent<ListLabels>();
        
        

        #if UNITY_EDITOR_WIN
            FileBrowser.lastPath="D:/llg/虚拟现实/mobile/vrml/model";
            //FileBrowser.lastPath="D:/pdf/vrml/model";
        #elif UNITY_ANDROID
            if(PlayerPrefs.HasKey("lastPath")) {
                FileBrowser.lastPath=PlayerPrefs.GetString("lastPath");
            }
        #endif

        

        

        camera = Camera.main;
        light = GameObject.Find("Directional Light").GetComponent<Light>();
        canvasJoyStick=GameObject.Find("CanvasJoyStick");
        lightOriIntensity = light.intensity;

        #if UNITY_STANDALONE
            useJoyStick=false;
        #else
            useJoyStick = true;
        #endif
    }

    void Start () {

        cameraInitialPosition=camera.transform.position;
        cameraInitialEulerAngles=camera.transform.eulerAngles;
        cameraInitialFieldOfView=camera.fieldOfView;

        cameraLight = GameObject.Find("Camera Light").GetComponent<Light>();
        cameraToLight = cameraLight.transform.position - camera.transform.position;
        
        transform.Find("InputField").gameObject.SetActive(true);
        transform.Find("PlotButton").gameObject.SetActive(true);
        listLabels.gameObject.SetActive(false);
        transform.Find("PathField").gameObject.SetActive(false);
        transform.Find("LoadButton/Text").GetComponent<Text>().text="Load";

        //canvasJoyStick=GameObject.FindObjectOfType<Canvas>().gameObject;
        
        onPlot();
        //onSelectFile(@"D:/llg/虚拟现实/vrml/sushemanyou/sushemanyou.WRL");
        //onSelectFile(@"D:\llg\虚拟现实\vrml\3_737\3\3\实验3\ly.WRL");

        transform.Find("InputField").GetComponent<InputField>().onEndEdit.AddListener(delegate(string str){
            if(model3d!=null && model3d.functionName==str) {
                return;
            }
            onPlot();
        });

        canvasJoyStick.transform.Find("JoyStickPad").GetComponent<EventTrigger>().triggers[0].callback.AddListener(onJoyStickPressed);
        canvasJoyStick.transform.Find("AscendButton").GetComponent<EventTrigger>().triggers[0].callback.AddListener(onAscendButtonPressed);
        canvasJoyStick.transform.Find("DescendButton").GetComponent<EventTrigger>().triggers[0].callback.AddListener(onDescendButtonPressed);
        transform.Find("LightButton").gameObject.GetComponent<Button>().onClick.AddListener(LightFollowCamera);
    }

    void onPlot() {
        string func=transform.Find("InputField").GetComponent<InputField>().text;

        int segCount=500;
        //int segCount=func.Split(new char[]{'|'})[0].Split(new char[]{','}).Length!=2?255:180;//not split
        if(model!=null) {
            GameObject.Destroy(model);
        }
        resetCamera();
        model3d=new Model3d(func,segCount,segCount,20,10,5);
        model=model3d.model;

        model.name="Model";
        model.transform.position=new Vector3(0,0,0);
		model.transform.localEulerAngles=new Vector3(0,0,0);
        for(int i = 0; i<model.transform.childCount; i++) {
            model.transform.GetChild(i).gameObject.name=model.transform.GetChild(i).gameObject.name.Replace("New Game Object","Model");
        }
        scaleLoaded=Mathf.Min(UnityEngine.Screen.width,UnityEngine.Screen.height)/(Mathf.Sqrt(UnityEngine.Screen.dpi*96f)*10f);
        resetScale();

        rx=0.0f;
        ry=0.0f;

        if(!receiveFunctionModelShadows) {
            setShadowVisible(model,false);
        }
        updateRotation();

        posLoaded=model.transform.position;

        transform.Find("InputField").gameObject.SetActive(!roaming && !listLabels.gameObject.activeSelf);
        transform.Find("PlotButton").gameObject.SetActive(!roaming && !listLabels.gameObject.activeSelf);
        transform.Find("PathField").gameObject.SetActive(listLabels.gameObject.activeSelf);
        transform.Find("LoadButton/Text").GetComponent<Text>().text=!listLabels.gameObject.activeSelf?"Load":"Back";

        canvasJoyStick.gameObject.SetActive(useJoyStick && roaming && !listLabels.gameObject.activeSelf);
        transform.Find("LightButton").gameObject.SetActive(canvasJoyStick.gameObject.activeSelf);
    }

    void onModel() {
        if(!listLabels.gameObject.activeSelf) {
            bool prefersBuildIn=true;
            #if UNITY_EDITOR
                prefersBuildIn=false;
            #endif
            FileBrowser.selectFile(listLabels,new string[]{"3ds","obj","wrl","wrz"},onSelectFile,"模型",prefersBuildIn);
            listLabels.onClickItem+=delegate{updateCurrentUrl();};
            updateCurrentUrl();
        } else {
            listLabels.gameObject.SetActive(false);
        }
        
        transform.Find("InputField").gameObject.SetActive(!roaming && !listLabels.gameObject.activeSelf);
        transform.Find("PlotButton").gameObject.SetActive(!roaming && !listLabels.gameObject.activeSelf);
        transform.Find("PathField").gameObject.SetActive(listLabels.gameObject.activeSelf);
        transform.Find("PathField").gameObject.SetActive(listLabels.gameObject.activeSelf);
        
        transform.Find("LoadButton/Text").GetComponent<Text>().text=!listLabels.gameObject.activeSelf?"Load":"Back";

        
        canvasJoyStick.gameObject.SetActive(useJoyStick && roaming && !listLabels.gameObject.activeSelf);
        transform.Find("LightButton").gameObject.SetActive(canvasJoyStick.gameObject.activeSelf);
    }

    void onSelectFile(string path) {


        if(model!=null) {
            GameObject.Destroy(model);
        }
        if(model3d!=null) {
            model3d=null;
        }
        
        if(path==null || path=="") {
            return;
        }
        listLabels.gameObject.SetActive(false);
        
        
        scaleLoaded=Mathf.Min(UnityEngine.Screen.width,UnityEngine.Screen.height)/(Mathf.Sqrt(UnityEngine.Screen.dpi*96f)*10f);
        resetCamera();
        ModelLoader.transparentMaterialSample = transparentMaterial;
        #if UNITY_EDITOR
            model=ModelLoader.loadFile(path,30,true,false,onSetCamera);
        #else
        try{
            model=ModelLoader.loadFile(path,30,true,false,onSetCamera);
        }catch{
            if(model!=null) {
                GameObject.Destroy(model);
            }
            model=new GameObject();
        }
        #endif

        #if UNITY_ANDROID
            PlayerPrefs.SetString("lastPath",FileBrowser.lastPath);
        #endif

        model.name="Model";
        for(int i = 0; i<model.transform.childCount; i++) {
            model.transform.GetChild(i).gameObject.name=model.transform.GetChild(i).gameObject.name.Replace("New Game Object","Model");
        }
        if(!receiveExternalModelShadows && !roaming) {
            setShadowVisible(model,false);
        }
        if(!receiveExternalSceneShadows && roaming) {
            setShadowVisible(model,false);
        }

        //scaleLoaded=Mathf.Min(UnityEngine.Screen.width,UnityEngine.Screen.height)/(UnityEngine.Screen.dpi*10f);
        scaleLoaded=model.transform.localScale.y;
        if (roaming)
        {
            scaleLoaded *= sceneScale;
        }
        resetScale();

        rx=0.0f;
        ry=0.0f;
        updateRotation();

        posLoaded=model.transform.position;

        

        transform.Find("InputField").gameObject.SetActive(!roaming && !listLabels.gameObject.activeSelf);
        transform.Find("PlotButton").gameObject.SetActive(!roaming && !listLabels.gameObject.activeSelf);
        transform.Find("PathField").gameObject.SetActive(listLabels.gameObject.activeSelf);
        transform.Find("LoadButton/Text").GetComponent<Text>().text=!listLabels.gameObject.activeSelf?"Load":"Back";

        canvasJoyStick.gameObject.SetActive(useJoyStick && roaming && !listLabels.gameObject.activeSelf);
        transform.Find("LightButton").gameObject.SetActive(canvasJoyStick.gameObject.activeSelf);
    }

    void updateCurrentUrl() {
        if(listLabels.gameObject.activeSelf && transform.Find("BrowsePanel").GetComponent<FileBrowser>()!=null) {
            string path=transform.Find("BrowsePanel").GetComponent<FileBrowser>().currentPath;
            Text text=transform.Find("PathField/Text").GetComponent<Text>();
            text.text=" "+path+" ";
            Canvas.ForceUpdateCanvases();
            transform.Find("PathField").GetComponent<ScrollRect>().horizontalNormalizedPosition=1f;
        }
    }

    void setShadowVisible(GameObject model,bool visible) {
        if(model==null) {
            return;
        }
        MeshRenderer mr=model.GetComponent<MeshRenderer>();
        if(mr!=null) {
            mr.receiveShadows=visible;
        }
        for(int i = 0; i<model.transform.childCount; i++) {
            setShadowVisible(model.transform.GetChild(i)?.gameObject,visible);
        }
    }

    void resetCamera() {
        roaming=false;
        camera.transform.position=cameraInitialPosition;
        camera.transform.eulerAngles=cameraInitialEulerAngles;
        camera.fieldOfView=cameraInitialFieldOfView;
        light.shadows=LightShadows.Soft;
        light.intensity = lightOriIntensity;
        if (cameraLight!=null)
        {
            cameraLight.gameObject.SetActive(false);
        }
        Cursor.lockState=CursorLockMode.None;
        Cursor.visible = true;
        transform.Find("LoadButton").gameObject.SetActive(true);
        #if !UNITY_STANDALONE
            camera.nearClipPlane=mobileScanNear;
            camera.farClipPlane=mobileScanFar;
        #endif

        resetVirtualKeyStates();
        LightFollowCamera();
    }

    void onSetCamera(Vector3 pos,Vector3 ang,float fieldOfView) {
        roaming=true;
        camera.transform.position=pos*sceneScale;
        camera.transform.eulerAngles=ang;
        camera.fieldOfView=fieldOfView;
        light.shadows=LightShadows.None;
        if (receiveExternalSceneShadows)
        {
            
            light.intensity = lightOriIntensity*0.5f;
            cameraLight.gameObject.SetActive(true);
        }
        #if !UNITY_STANDALONE
            camera.nearClipPlane=mobilePanoNear;
            camera.farClipPlane=mobilePanoFar;
        #endif
        resetVirtualKeyStates();
        #if UNITY_STANDALONE
            Cursor.lockState = CursorLockMode.Locked;
            transform.Find("LoadButton").gameObject.SetActive(false);
            Cursor.visible = false;
        #endif
        LightFollowCamera();
    }

    void resetVirtualKeyStates() {
        pressedJoyStick=false;
        pressedAscendButton=false;
        pressedDescendButton=false;

        pressedJoyStickPos=new Vector2(0,0);
        pressedJoyStickTouchId=-1;
        pressedLiftTouchId=-1;
    }

    public static int GetNearestTouchId(Vector2 position)
    {
        int touchCount = Input.touchCount;
        float distmin = float.MaxValue;
        int fingerId = -1;
        for (int i = touchCount - 1; i >= 0; i--)
        {
            Touch touch = Input.GetTouch(i);
            float dist = (touch.position - position).sqrMagnitude;
            if (dist < distmin)
            {
                distmin = dist;
                fingerId = touch.fingerId;
            }
        }
        return fingerId;
    }

    public static int GetTouchId(PointerEventData eventData)
    {
        return GetNearestTouchId(eventData.pressPosition);
    }
    public void onJoyStickPressed(BaseEventData data) {
        Vector2 point0=new Vector2(0,0);
        Vector2 mousePosition=((PointerEventData)data).pressPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasJoyStick.transform.Find("JoyStickPad").GetComponent<RectTransform>(),
            mousePosition,null,out point0);

        if(point0.magnitude>joyStickRMax) {
            return;
        }

        pressedJoyStickPos=point0;
        pressedJoyStickTouchId=GetTouchId((PointerEventData)data);

        pressedJoyStick=true;

    }

    public void onAscendButtonPressed(BaseEventData data) {
        pressedAscendButton=true;
        pressedDescendButton=false;

        pressedLiftTouchId=GetTouchId((PointerEventData)data);
    }

    public void onDescendButtonPressed(BaseEventData data) {
        pressedAscendButton=false;
        pressedDescendButton=true;

        pressedLiftTouchId=GetTouchId((PointerEventData)data);
    }

    public void onAscendButtonEntered() {
        if(pressedAscendButton || pressedDescendButton) {
            pressedAscendButton=true;
            pressedDescendButton=false;
        }
    }

    public void onDescendButtonEntered() {
        if(pressedAscendButton || pressedDescendButton) {
            pressedAscendButton=false;
            pressedDescendButton=true;
        }
    }

    void resetScale() {
        if(model!=null) {
            float scale=scaleLoaded;
            model.transform.localScale=new Vector3(scale,scale,scale);
        }
    }



    void updateRotation() {
        if(model!=null) {
            model.transform.localEulerAngles=new Vector3(0,0,0);
            
            model.transform.Rotate(new Vector3(1,0,0),rx);
            model.transform.Rotate(new Vector3(0,1,0),ry);
        }
    }

    void LightFollowCamera()
    {
        if (cameraLight.transform.parent != camera.transform)
        {
            cameraLight.transform.position = camera.transform.position + cameraToLight;
        }
    }
	
	// Update is called once per frame
	void Update () {
        
        if(listLabels.gameObject.activeSelf || model==null) {
            
            return;
        }
        

        if (!roaming) {
            if(Input.touchCount<=1 && Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonUp(0) && !hasScaleOri && !RectTransformUtility.RectangleContainsScreenPoint(transform.Find("InputField").GetComponent<RectTransform>(), Input.mousePosition) && !RectTransformUtility.RectangleContainsScreenPoint(transform.Find("PlotButton").GetComponent<RectTransform>(), Input.mousePosition)) {
                ry+=-5.0f*Input.GetAxis("Mouse X");
                rx+=5.0f*Input.GetAxis("Mouse Y");
                updateRotation();
            }
            if(Input.GetAxis("Mouse ScrollWheel")!=0) {
                float scale0=model.transform.localScale.y;
                float scale=Input.GetAxis("Mouse ScrollWheel")>0?scale0*1.1f:scale0/1.1f;
                model.transform.localScale=new Vector3(scale,scale,scale);
            }

            if(Input.touchCount==2) {
            
                Vector4 scalePosCur=new Vector4(Input.GetTouch(0).position.x,Input.GetTouch(0).position.y,Input.GetTouch(1).position.x,Input.GetTouch(1).position.y);
                if(!hasScaleOri) {
                    scaleOri=model.transform.localScale.y;
                    scalePosOri=scalePosCur;
                    hasScaleOri=true;
                }
                float dx0=scalePosOri.z-scalePosOri.x;
                float dy0=scalePosOri.w-scalePosOri.y;
                float dx=scalePosCur.z-scalePosCur.x;
                float dy=scalePosCur.w-scalePosCur.y;
                float d0=Mathf.Sqrt(dx0*dx0+dy0*dy0);
                float d=Mathf.Sqrt(dx*dx+dy*dy);
                if(d0>0 && d0>=5*Mathf.Sqrt(Screen.dpi)) {
                    float scale=scaleOri*d/d0;
                    model.transform.localScale=new Vector3(scale,scale,scale);
                    updateRotation();
                }
            } else {
                hasScaleOri=false;
            }
            if(Input.touchCount==3) {
                resetScale();
                rx=0.0f;
                ry=0.0f;
                updateRotation();
                model.transform.position=posLoaded;
            }
        } else if(camera!=null){
            #if UNITY_STANDALONE
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.LeftCommand) || Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None;
                transform.Find("LoadButton").gameObject.SetActive(Cursor.lockState == CursorLockMode.None);
                Cursor.visible= Cursor.lockState == CursorLockMode.None;
            }
            #endif
            bool inPanoCondition=!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonUp(0);

            #if UNITY_STANDALONE || UNITY_EDITOR
                inPanoCondition=inPanoCondition && (Input.GetMouseButton(0) || Cursor.lockState == CursorLockMode.Locked);
            #else
                inPanoCondition=inPanoCondition && Input.touchCount>0;
            #endif
            
            Vector3 r0=camera.transform.eulerAngles;
            if(inPanoCondition) {
                if(Input.touchCount==0 && !pressedJoyStick && !pressedAscendButton && !pressedDescendButton) {
                    r0.x-=Input.GetAxis("Mouse Y")*5/(Screen.dpi/96f);
                    r0.y+=Input.GetAxis("Mouse X")*5/(Screen.dpi/96f);
                } else {
                    for (int i = 0; i < Input.touchCount; i++){
                        if (Input.GetTouch(i).fingerId!=pressedJoyStickTouchId && Input.GetTouch(i).fingerId!=pressedLiftTouchId && Input.GetTouch(i).deltaPosition.magnitude<Screen.dpi*0.2f)
                        {

                            r0.x-=Input.GetTouch(i).deltaPosition.y/(Screen.dpi/96f);
                            r0.y+=Input.GetTouch(i).deltaPosition.x/(Screen.dpi/96f);
                            break;
                        }
                    }
                }
                if(r0.x>180f) {
                    r0.x-=360f;
                }
                r0.x=Mathf.Max(-90f,Mathf.Min(90f,r0.x));
                camera.transform.eulerAngles=r0;
            }

            Vector3 p0=camera.transform.position;
            float speed=keyboardVMax;
            if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.CapsLock) || Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.F)){
                speed/=10f;
            }else if(Input.GetKey(KeyCode.Tab) || Input.GetKey(KeyCode.Q)){
                speed*=10f;
            }

            bool willChangePosition=false;
            if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
                p0.x+=Mathf.Sin(r0.y*Mathf.PI/180f)*speed;
                p0.z+=Mathf.Cos(r0.y*Mathf.PI/180f)*speed;
                willChangePosition=true;
            }
            if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
                p0.x-=Mathf.Sin(r0.y*Mathf.PI/180f)*speed;
                p0.z-=Mathf.Cos(r0.y*Mathf.PI/180f)*speed;
                willChangePosition=true;
            }
            if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
                p0.x-=Mathf.Cos(r0.y*Mathf.PI/180f)*speed;
                p0.z+=Mathf.Sin(r0.y*Mathf.PI/180f)*speed;
                willChangePosition=true;
            }
            if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
                p0.x+=Mathf.Cos(r0.y*Mathf.PI/180f)*speed;
                p0.z-=Mathf.Sin(r0.y*Mathf.PI/180f)*speed;
                willChangePosition=true;
            }
            if(Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.PageUp)) {
                p0.y+=speed;
                willChangePosition=true;
            }
            if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.PageDown)){
                p0.y-=speed;
                willChangePosition=true;
            }
            

            if(pressedJoyStick) {
                Vector2 point0=new Vector2(0,0);
                Vector2 mousePosition=Input.mousePosition;
                Touch? joyStickTouch=null;
                if(pressedJoyStickTouchId>=0) {
                    for (int i = 0; i < Input.touchCount; i++){
                        if (Input.GetTouch(i).fingerId == pressedJoyStickTouchId){
                            mousePosition = Input.GetTouch(i).position;
                            joyStickTouch = Input.GetTouch(i);
                            break;
                        }
                    }
                }
                if(joyStickTouch==null) {
                    if(pressedJoyStickTouchId>=0 || Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) {
                        pressedJoyStick=false;
                        pressedJoyStickPos=new Vector2(0,0);
                        pressedJoyStickTouchId=-1;
                        canvasJoyStick.transform.Find("JoyStickPad/JoyStick").transform.localPosition=new Vector3(0,0,0);
                    }
                }
                if(pressedJoyStick) {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasJoyStick.transform.Find("JoyStickPad").GetComponent<RectTransform>(),
                    mousePosition,null,out point0);
                    float rmax=joyStickRMax;
                    Vector2 rate=new Vector2((point0.x-pressedJoyStickPos.x)/rmax,(point0.y-pressedJoyStickPos.y)/rmax);
                    if(rate.sqrMagnitude>1) {
                        rate=rate.normalized;
                    }
                    canvasJoyStick.transform.Find("JoyStickPad/JoyStick").transform.localPosition=new Vector3(rate.x*rmax,rate.y*rmax,0);
                    Vector2 v0=new Vector2(rate.x*rate.x*rate.x* joyStickVMax,rate.y*rate.y*rate.y*joyStickVMax);

                    p0.x+=Mathf.Sin(r0.y*Mathf.PI/180f)*v0.y+Mathf.Cos(r0.y*Mathf.PI/180f)*v0.x;
                    p0.z+=Mathf.Cos(r0.y*Mathf.PI/180f)*v0.y-Mathf.Sin(r0.y*Mathf.PI/180f)*v0.x;
                    willChangePosition=true;
                }
            }

            if(pressedAscendButton || pressedDescendButton) {
                Touch? liftTouch=null;
                if(pressedLiftTouchId>=0) {
                    for (int i = 0; i < Input.touchCount; i++){
                        if (Input.GetTouch(i).fingerId == pressedLiftTouchId){
                            liftTouch = Input.GetTouch(i);
                            break;
                        }
                    }
                }
                if(liftTouch==null) {
                    if(pressedLiftTouchId>=0 || Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) {
                        pressedAscendButton=false;
                        pressedDescendButton=false;
                        pressedLiftTouchId=-1;
                    }
                }
                if(pressedAscendButton) {
                    p0.y+=liftButtonV;
                    willChangePosition=true;
                }
                if(pressedDescendButton) {
                    p0.y-=liftButtonV;
                    willChangePosition=true;
                }
            }

            if(willChangePosition) {
                camera.transform.position=p0;
            }
            
            
            if(Input.GetKeyUp(KeyCode.L)){
                LightFollowCamera();
            }
        }
        
    }

    void CreateCube()
    {

        GameObject obj = new GameObject("cube");
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();


        Vector3[] vertices = new Vector3[24];
        int[] triangles = new int[36];

        //forward
        vertices[0].Set(0.5f, -0.5f, 0.5f);
        vertices[1].Set(-0.5f, -0.5f, 0.5f);
        vertices[2].Set(0.5f, 0.5f, 0.5f);
        vertices[3].Set(-0.5f, 0.5f, 0.5f);
        //back
        vertices[4].Set(vertices[2].x, vertices[2].y, -0.5f);
        vertices[5].Set(vertices[3].x, vertices[3].y, -0.5f);
        vertices[6].Set(vertices[0].x, vertices[0].y, -0.5f);
        vertices[7].Set(vertices[1].x, vertices[1].y, -0.5f);
        //up
        vertices[8] = vertices[2];
        vertices[9] = vertices[3];
        vertices[10] = vertices[4];
        vertices[11] = vertices[5];
        //down
        vertices[12].Set(vertices[10].x, -0.5f, vertices[10].z);
        vertices[13].Set(vertices[11].x, -0.5f, vertices[11].z);
        vertices[14].Set(vertices[8].x, -0.5f, vertices[8].z);
        vertices[15].Set(vertices[9].x, -0.5f, vertices[9].z);
        //right
        vertices[16] = vertices[6];
        vertices[17] = vertices[0];
        vertices[18] = vertices[4];
        vertices[19] = vertices[2];
        //left
        vertices[20].Set(-0.5f, vertices[18].y, vertices[18].z);
        vertices[21].Set(-0.5f, vertices[19].y, vertices[19].z);
        vertices[22].Set(-0.5f, vertices[16].y, vertices[16].z);
        vertices[23].Set(-0.5f, vertices[17].y, vertices[17].z);

        int currentCount = 0;
        for (int i = 0; i < 24; i = i + 4)
        {
            triangles[currentCount++] = i;
            triangles[currentCount++] = i + 3;
            triangles[currentCount++] = i + 1;

            triangles[currentCount++] = i;
            triangles[currentCount++] = i + 2;
            triangles[currentCount++] = i + 3;

        }

        mf.mesh.vertices = vertices;
        mf.mesh.triangles = triangles;
    }
}
