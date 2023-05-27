using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class FileBrowser : MonoBehaviour
{
    
    #if UNITY_STANDALONE_WIN
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenFileName
        {
            public int structSize = 0;
            public IntPtr dlgOwner = IntPtr.Zero;
            public IntPtr instance = IntPtr.Zero;
            public String filter = null;
            public String customFilter = null;
            public int maxCustFilter = 0;
            public int filterIndex = 0;
            public String file = null;
            public int maxFile = 0;
            public String fileTitle = null;
            public int maxFileTitle = 0;
            public String initialDir = null;
            public String title = null;
            public int flags = 0;
            public short fileOffset = 0;
            public short fileExtension = 0;
            public String defExt = null;
            public IntPtr custData = IntPtr.Zero;
            public IntPtr hook = IntPtr.Zero;
            public String templateName = null;
            public IntPtr reservedPtr = IntPtr.Zero;
            public int reservedInt = 0;
            public int flagsEx = 0;
        }

    
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
 
        //链接指定系统函数        另存为对话框
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        static extern bool GetSaveFileName([In, Out] OpenFileName ofn);

        public static string openFileOnWindows(string[] extNames,string extNameDesc="指定的文件")
        {
            OpenFileName openFileName = new OpenFileName();
            openFileName.structSize = Marshal.SizeOf(openFileName);
            openFileName.filter = extNameDesc+"(*."+string.Join(" *.",extNames)+")\0*."+string.Join(";*.",extNames)+"";
            openFileName.file = new string(new char[256]);
            openFileName.maxFile = openFileName.file.Length;
            openFileName.fileTitle = new string(new char[64]);
            openFileName.maxFileTitle = openFileName.fileTitle.Length;
            //openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
            openFileName.title = "选择"+extNameDesc;
            //openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
            openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
            if(GetOpenFileName(openFileName)) {
                return openFileName.file;
            } else {
                return "";
            }
        }
    #endif

    
    public static readonly string defaultPath=getDefaultPath();
    public static string lastPath=defaultPath;
    public delegate void SelectHandler(string pathName);
    //public delegate void CancelHandler();
    event SelectHandler onSelect=null;
    //event CancelHandler onCancel=null;
    
    public List<string> extNameArr=new List<string>();
    public string currentPath="";

    static string getDefaultPath() {
        #if UNITY_STANDALONE_WIN
            return Directory.GetCurrentDirectory();
        #elif UNITY_ANDROID
            //return "/storage/emulated/0/";
            return "/sdcard/";
            //return Application.persistentDataPath;
        #elif UNITY_IOS
            return Application.persistentDataPath;
        #else
            return "/Users/";
        #endif
    }

    public static void requestPermissions() {
        #if UNITY_ANDROID
            UnityEngine.Android.Permission.RequestUserPermission("android.permission.READ_EXTERNAL_STORAGE");
            //UnityEngine.Android.Permission.RequestUserPermission("android.permission.WRITE_EXTERNAL_STORAGE");
        #endif
    }

    /// <summary>
    /// 比较可能有编号的字符串s1和s2，不优先区分大小写。
    /// </summary>
    /// <returns>-1:s1<s2 1:s1>s2 0:未确定</returns>
    public static int strnumcmp(string s1,string s2){
	    //
	    int i=0;
	    bool nummode1=false;
	    bool nummode2=false;
	    int numcmp=-2;
	    char c01;
	    char c02;
	    char c1;
	    char c2;
	    int casecmp=0;
        int leng=s1.Length<=s2.Length?s1.Length:s2.Length;
	    for(i=0;i<leng;i++){
		    c01=s1[i];
		    c02=s2[i];
		    if(c01>='0' && c01<='9'){
			    nummode1=true;
		    }else{
			    nummode1=false;
		    }
		    if(c02>='0' && c02<='9'){
			    nummode2=true;
		    }else{
			    nummode2=false;
		    }
		    if(nummode1 && nummode2 && numcmp==-2){
			    numcmp=0;
		    }
		    if(numcmp==0){
			    if(c01>c02){
				    numcmp=1;
			    }else if(c01<c02){
				    numcmp=-1;
			    }
		    }
		    if(numcmp>-2){
			    if(nummode1 && !nummode2){
				    return 1;
			    }else if(!nummode1 && nummode2){
				    return -1;
			    }else if(!nummode1 && !nummode2){
				    if(numcmp!=0){
					    return numcmp;
				    }
			    }else{
				    continue;
			    }
		    }
		    if(!nummode1 || !nummode2){
			    numcmp=-2;
		    }
		    if(c01>='A' && c01<='Z'){
			    c1=(char)((int)c01+32);
		    }else{
			    c1=c01;
		    }
		    if(c02>='A' && c02<='Z'){
			    c2=(char)((int)c02+32);
		    }else{
			    c2=c02;
		    }
		    if(c1>c2){
			    return 1;
		    }else if(c1<c2){
			    return -1;
		    }else if(casecmp==0){
			    if(c01>c02){
				    casecmp=1;
			    }else if(c01<c02){
				    casecmp=-1;
			    }
		    }
	    }
	    if(nummode1 && nummode2){
		    if(s1.Length>leng){
			    return 1;
		    }else if(s2.Length>leng){
			    return -1;
		    }else if(numcmp==1){
			    return 1;
		    }else if(numcmp==-1){
			    return -1;
		    }else{
			    return casecmp;
		    }
	    }else{
		    if(s1.Length>leng){
			    return 1;
		    }else if(s2.Length>leng){
			    return -1;
		    }else{
			    return casecmp;
		    }
	    }
	    //return 0;
    }

    public static List<string> getPathNameListInDir(string dirpath=""){
	    if(dirpath==null || dirpath=="")dirpath=lastPath;
        if(dirpath==null || dirpath=="")return new List<string>();
        List<string> result=new List<string>();

        dirpath=dirpath.Replace("\\","/");
        if(dirpath[dirpath.Length-1]!='/') {
            dirpath+="/";
        }

        try {
            requestPermissions();
            if (Directory.Exists(dirpath)){  
                DirectoryInfo direction = new DirectoryInfo(dirpath);
                DirectoryInfo[] dirs = direction.GetDirectories();
                FileInfo[] files = direction.GetFiles("*",SearchOption.TopDirectoryOnly);
                
                for(int i=0;i<dirs.Length;i++){
                    string pathName=dirs[i].FullName.Replace("\\","/");
                    result.Add(pathName+"/");
                }
                for(int i=0;i<files.Length;i++){
                    string pathName=files[i].FullName.Replace("\\","/");
                    result.Add(pathName);
                }
            }
            result.Sort((string s1,string s2) => {
                if(s1.Length>0 && s2.Length>0) {
                    if(s1[s1.Length-1]=='/' && s2[s2.Length-1]!='/') {
                        return -1;
                    }
                    if(s1[s1.Length-1]!='/' && s2[s2.Length-1]=='/') {
                        return 1;
                    }
                }
                return strnumcmp(s1,s2);
            });
        } catch(Exception ex) {
            Debug.Log(ex);
        }
        return result;
    }

    public static void selectFile(ListLabels listLabels,string[] extNames=null,SelectHandler onSelectFunc=null,string extNameDesc="指定的文件",bool prefersBuildIn=true){
        if(prefersBuildIn) {
            #if UNITY_STANDALONE_WIN
                string pathNameOnWindows=openFileOnWindows(extNames,extNameDesc);
                if(pathNameOnWindows!=null && pathNameOnWindows!="") {
                    onSelectFunc?.Invoke(pathNameOnWindows);
                }
                return;
            #endif
        }
        
        if(listLabels==null) {
            return;
        }
        GameObject obj=listLabels.gameObject;
        if(obj==null) {
            return;
        }
        obj.SetActive(true);

        FileBrowser browser=obj.GetComponent<FileBrowser>();
        if(browser==null) {
            browser=obj.AddComponent<FileBrowser>();
        }
        browser.onSelect=onSelectFunc;
        browser.currentPath=lastPath;
        browser.extNameArr=extNames!=null?new List<string>(extNames):new List<string>();
        
        listLabels.setClickEvent(browser.onSelectItem);
        browser.updateFileList();
    }

    void onSelectItem(int index) {
        ListLabels listLabels=GetComponent<ListLabels>();
        if (listLabels!=null && index>=0 && index<listLabels.data.Count) {
            string path=listLabels.data[index];
			bool isFolder=false;
			if(path.Length>=2 && path[path.Length-1]=='/' || path=="/"){
				isFolder=true;
			}
            if(isFolder || path.Length==0) {
                currentPath=path;
                updateFileList();
            } else {
                lastPath=currentPath;
                onSelect?.Invoke(path);
            }
        }
    }

    void updateFileList(){
        string path0=currentPath.Replace("\\","/");
        
        string parentPath=path0;
	    if(parentPath.Length>0 && parentPath!="/"){
		    if(parentPath[parentPath.Length-1]=='/'){
			    parentPath=parentPath.Substring(0, parentPath.Length-1);
		    }
		    parentPath=parentPath.Substring(0, parentPath.LastIndexOf("/")+1);
	    }
        //Debug.Log(path0);
        //Debug.Log(parentPath);

        int i=0;
	    int j=0;
        List<string> pathNameArray=new List<string>();
        List<string> texts=new List<string>();
	    if(path0.Length>0){
            pathNameArray=getPathNameListInDir(path0);
            int iStart = 1;
		    if(path0!="/"){
                texts.Add("<上一级目录>");
                pathNameArray.Insert(0,parentPath);
            }
            else
            {
                iStart = 0;
            }
		    for(i=iStart; i<pathNameArray.Count;i++){
			    string path=pathNameArray[i];
			    if(path==null || path=="" || path=="../")continue;
			    bool isFolder=false;
			    if(path.Length>=2 && path[path.Length-1]=='/'){
				    isFolder=true;
			    }
			    string fileName=isFolder?path.Substring(path.LastIndexOf('/',path.Length-2)+1):path.Substring(path.LastIndexOf('/')+1);
			
			    if(!isFolder && extNameArr.Count>0){
				    int extNamePos=path.LastIndexOf(".");
				    string extName=extNamePos>0?path.Substring(extNamePos+1).ToLower():"<none>";
				    for(j=0;j<extNameArr.Count;j++){
					    if(extNameArr[j]==extName){
						    break;
					    }
				    }
				    if(j>=extNameArr.Count){
                        pathNameArray.RemoveAt(i);
                        i--;
					    continue;
				    }
			    }
			    texts.Add(fileName);
		    }
	    }else{
		    int charCode_A=(int)'A';
		    for(i=0;i<26;i++){
			    string path=(char)(charCode_A+i)+":/";
			    string fileName=(char)(charCode_A+i)+":";
			    pathNameArray.Add(path);
                texts.Add(fileName);
		    }
	    }

        ListLabels listLabels=GetComponent<ListLabels>();
        if(listLabels!=null) {
            listLabels.texts=texts;
            listLabels.data=pathNameArray;
            listLabels.refresh();
        }
    }

    void Start()
    {
        //updateFileList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
