using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListLabels : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("每个单元格的底图")]
    public Sprite gridImage=null;
    [Header("字体")]
    public Font font=null;
    [Header("字号(文字高度为字号的一半)")]
    public float fontSize=60;
    [Header("颜色")]
    public Color color=new Color(0,0,0,1);
    [Header("单元格高度与文字高度的比值")]
    public float lineHeightRate=1.25f;
    [Header("每个单元格底图的透明度")]
    [Range(0f,1f)]
    public float gridAlpha=0.75f;
    [Header("单元格边距")]
    public float padding=5f;
    [Header("单元格间距")]
    public float spacing=5f;
    [Header("字符缩进量(相对于字号)")]
    public float leading=30f;
    const float textScale=0.5f;
    
    public List<string> texts=new List<string>();
    public List<string> data=new List<string>();

    public delegate void ClickHandler(int index);
    public event ClickHandler onClickItem=null;

    GameObject content=null;
    Vector2 oriSize=new Vector2(1,1);

    public void removeClickEvents() {
        onClickItem=null;
    }
    public void setClickEvent(ClickHandler handler) {
        onClickItem=handler;
    }

    void Start()
    {
        if(GetComponent<Text>()==null && GetComponent<Image>()==null) {
            gameObject.AddComponent<Text>();
            GetComponent<Text>().text="";
        }
        if(GetComponent<ScrollRect>()==null) {
            gameObject.AddComponent<ScrollRect>();
        }
        if(GetComponent<RectMask2D>()==null) {
            gameObject.AddComponent<RectMask2D>();
        }
        
        ScrollRect scrollRect=GetComponent<ScrollRect>();
        scrollRect.vertical=true;
        scrollRect.horizontal=false;


        content=new GameObject();
        content.name="ListLabelsContent";
        
        RectTransform rt=content.AddComponent<RectTransform>();
        content.transform.SetParent(transform);
        scrollRect.content=rt;

        rt.pivot=new Vector2(0.5f,0.5f);
        rt.anchorMin=new Vector2(0.5f,0.5f);
        rt.anchorMax=new Vector2(0.5f,0.5f);
        oriSize=gameObject.GetComponent<RectTransform>().rect.size;
        rt.sizeDelta=oriSize;
        rt.anchoredPosition=new Vector2(0,0);
        refresh();
    }

    public void refresh() {
        if(content==null) {
            return;
        }
        for(int i=content.transform.childCount-1;i>=0;i--) {
            GameObject.Destroy(content.transform.GetChild(i).gameObject);
        }
        oriSize=gameObject.GetComponent<RectTransform>().rect.size;
        RectTransform rt=content.GetComponent<RectTransform>();
        rt.sizeDelta=oriSize;
        for(int i = 0; i<texts.Count; i++) {
            addItem(texts[i],i);
        }
        rt.anchoredPosition=new Vector2(0,oriSize.y*0.5f-rt.sizeDelta.y*0.5f);
    }

    GameObject addItem(string strText,int index) {
        GameObject obj=new GameObject();
        RectTransform rt=obj.AddComponent<RectTransform>();
        obj.transform.SetParent(content.transform);
        rt.pivot=new Vector2(0.5f,0.5f);
        rt.anchorMin=new Vector2(0.5f,1f);
        rt.anchorMax=new Vector2(0.5f,1f);
        rt.sizeDelta=new Vector2(oriSize.x-2*padding,Mathf.Round(fontSize*textScale*lineHeightRate));
        
        obj.name="item"+(index+1);
        rt.anchoredPosition=new Vector2(0,-padding-rt.sizeDelta.y*0.5f-(rt.sizeDelta.y+spacing)*index);
        content.GetComponent<RectTransform>().sizeDelta=new Vector2(oriSize.x,Mathf.Max(oriSize.y,
            2*padding+rt.sizeDelta.y+(rt.sizeDelta.y+spacing)*index));

        Image img=obj.AddComponent<Image>();
        img.sprite=gridImage;
        img.type=Image.Type.Sliced;
        img.fillCenter=true;
        img.color=new Color(1,1,1,gridAlpha);

        Button btn=obj.AddComponent<Button>();
        btn.onClick.AddListener(delegate{onItemClicked(index);});

        GameObject txtObj=new GameObject();
        txtObj.name="txt"+(index+1);
        RectTransform rt1=txtObj.AddComponent<RectTransform>();
        txtObj.transform.SetParent(obj.transform);
        rt1.pivot=new Vector2(0.5f,0.5f);
        rt1.anchorMin=new Vector2(0.5f,0.5f);
        rt1.anchorMax=new Vector2(0.5f,0.5f);
        rt1.localScale=new Vector3(textScale,textScale,1);
        rt1.sizeDelta=new Vector2(rt.sizeDelta.x/textScale-leading,rt.sizeDelta.y/textScale);
        rt1.anchoredPosition=new Vector2(leading*textScale*0.5f,0);

        Text txt=txtObj.AddComponent<Text>();
        txt.text=strText;
        txt.font=font;
        txt.color=color;
        txt.fontSize=Mathf.RoundToInt(fontSize);
        txt.alignment=TextAnchor.MiddleLeft;

        

        return obj;
    }

    void onItemClicked(int index) {
        onClickItem?.Invoke(index);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
