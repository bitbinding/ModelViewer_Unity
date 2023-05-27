using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

//Converted from ModelLoader.as
public class ModelLoader{
	public static string shaderName="Custom/CullOffShader";
	public static Material transparentMaterialSample = null;
	public delegate void SetCameraHandler(Vector3 position,Vector3 eulerAngles,float fieldOfView);
	//public static byte[] ulstr=null;
	//public static bool utfForceMode=false;
	//public static bool preRemoveComment=true;
	//public List<List<Object>> urlModelMapping=null;
	//public List<List<Object>> textureUsingArray=null;
    /// <summary>
    ///  constructor code
    /// </summary>
	public ModelLoader(){
	}
	
	public static GameObject loadFile(string fname,float angleLimit=30f,bool moveToCenter=true,bool forceAutoUV=false,SetCameraHandler onSetCamera=null){
		GameObject obj=new GameObject();
		(new ModelLoader()).loadFileToModel(obj,fname,angleLimit,moveToCenter,forceAutoUV,onSetCamera);
		return obj;
	}
	
	public void loadFileToModel(GameObject obj,string fname,float angleLimit=30f,bool moveToCenter=true,bool forceAutoUV=false,SetCameraHandler onSetCamera=null){
		int i=0;
		
		string[] fnameArray=fname.Split(new char[]{'|'});
		string fext=fnameArray[0].Substring(fnameArray[0].LastIndexOf(".")+1);
		string modelFileStr=fnameArray[0];
		string bitmapFileStr=fnameArray.Length>1?fnameArray[1]:"";
		modelFileStr=modelFileStr.Replace("\\","/");
		bitmapFileStr=bitmapFileStr.Replace("\\","/");
		ModelLoader modelLoader0=this;
		byte[] data;
		if(fext.ToLower()=="3ds"){
			data=readFile(modelFileStr);
			load3dsFile(data,obj,angleLimit,moveToCenter,forceAutoUV);
		}else if(fext.ToLower()=="wrl" || fext.ToLower()=="wrz"){
			data=readFile(modelFileStr);
			loadVrmlFile(modelFileStr,data,obj,angleLimit,moveToCenter,forceAutoUV,onSetCamera);
		}else if(fext.ToLower()=="obj"){
			data=readFile(modelFileStr);
			loadObjFile(data,obj,angleLimit,moveToCenter,forceAutoUV);
		}else{
			throw new NotSupportedException("该模型文件不支持，仅支持3ds,wrl,obj格式文件");
		}
		if(bitmapFileStr!=""){
			loadBitmapTextureToObj(bitmapFileStr,obj);
		}
	}
	
	byte[] readFile(string url)
    {
        FileStream fileStream=null;
        try {
            fileStream = new FileStream(url, FileMode.Open, FileAccess.Read);
        } catch(IOException ex) {
            Debug.Log(ex.Message);
            fileStream=null;
        }

        if(fileStream==null) {
            return new byte[0];
        }
        
        fileStream.Seek(0, SeekOrigin.Begin);
        //创建文件长度缓冲区
        byte[] bytes = new byte[fileStream.Length];
        //读取文件
        fileStream.Read(bytes, 0, (int)fileStream.Length);
        //释放文件读取流
        fileStream.Close();
        fileStream.Dispose();
        fileStream = null;
		return bytes;
    }
	
	void loadBitmapTextureToObj(string url,GameObject obj,Dictionary<string,Material> urlToMaterialMap=null,float alpha=1){
		
		Texture2D texture;
		string urlKey = url;
		if (alpha < 1)
		{
			urlKey += "|alpha" + (int) (alpha * 255);
		}
		if (urlToMaterialMap != null && urlToMaterialMap.ContainsKey(urlKey))
		{
			obj.GetComponent<MeshRenderer>().material = urlToMaterialMap[urlKey];
			return;
		}
		
		texture=TextureLoader.load(url);
		if(texture==null) {
			return;
		}
		
		
		
		MeshRenderer mr=obj.GetComponent<MeshRenderer>();
		if(mr!=null) {
			mr.material.color=new Color(1,1,1,alpha);
			mr.material.mainTexture=texture;
			if (urlToMaterialMap!=null)
			{
				urlToMaterialMap.Add(urlKey,mr.material);
			}
		}
	}
	
	
	
	
	
    /// <summary>
    /// 加载3ds文件
    /// 读取3ds文件，在十六进制编辑器中，3ds文件顶点根节点关键字为1041(0x4110，3ds的记录方式是低字节在前，高字节在后的，与byte[]存储的方向相反)，后4字节为该区块的长度，再后2字节为顶点的数目（<65536），之后为xyz顶点坐标集合，数据均为float型，也与byte[]存储的方向相反，占4位。
    /// 3ds文件面片的根节点关键字为2041(0x4120)，后4字节为该区块的长度，再后2字节为面片的数目（<65536）（低位在前，高位在后），之后为顶点1、顶点2、顶点3和附加信息的序号集合，数据均为short型。
	/// 加载完成后将y和z轴调换
    /// </summary>
	void load3dsFile(byte[] ulbin,GameObject obj,float angleLimit=30f,bool moveToCenter=true,bool forceAutoUV=false){
		byte[] floatbin=new byte[4];//浮点数的暂存空间
		int leng=ulbin.Length;//文件的字节数
		int leng2=0;//顶点数
		int leng3=0;//面数
		if(leng<6 || ulbin[0]!=0x4d || ulbin[1]!=0x4d || (ulbin[5]<<24|ulbin[4]<<16|ulbin[3]<<8|ulbin[2])!=leng){
			UnityEngine.Debug.Log("该文件不是3ds文件");
			return;
		}
		int readFlag=-3;//确定是读取文件模式（正奇数）还是找寻关键点模式（偶数或负数）
		Vector3[] point=new Vector3[0];
		int[] coord=new int[0];
		Vector2[] uv=new Vector2[0];
		int i=0;
		int j=0;
		int k=0;
		int l=0;
		float pointMax=0.1f;
		float avax=0;
		float avay=0;
		float zmin=float.PositiveInfinity;
		for(i=0;i+8<leng;i++){
			if(readFlag==-3 && ulbin[i]==0x3d && ulbin[i+1]==0x3d){
				readFlag=-2;//主编辑块
			}
			if(readFlag==-2 && ulbin[i]==0x00 && ulbin[i+1]==0x40){
				readFlag=-1;//物体块
			}
			if(readFlag==-1 && ulbin[i]==0x00 && ulbin[i+1]==0x41){
				readFlag=0;//网格块
			}
			if(readFlag==0 && ulbin[i]==0x10 && ulbin[i+1]==0x41){
				//顶点列表块
				i+=6;
				readFlag=1;
				leng2=(ulbin[i+1]<<8|ulbin[i]);
				point=new Vector3[leng2];
				//pointt=new Array(leng2);
				i+=2;
				j=0;
				k=0;
				
			}
			if(readFlag==1){
				//读取顶点列表
				floatbin[0]=ulbin[i];
				floatbin[1]=ulbin[i+1];
				floatbin[2]=ulbin[i+2];
				floatbin[3]=ulbin[i+3];
				l=k>0?3-k:k;
				point[j][l]=BitConverter.ToSingle(floatbin,0);
				k++;
				if(k>=3){
					k=0;
					j++;
				}
				if(j>=leng2){
					readFlag=2;
				}
				i+=3;
			}
			if(readFlag==2){
				//面信息列表块
				if(ulbin[i]==0x11 && ulbin[i+1]==0x41){
					
				}else if(ulbin[i]==0x40 && ulbin[i+1]==0x41){
					i+=6;
					readFlag=4;
					uv=new Vector2[leng2];
					i+=2;
					j=0;
					k=0;
				}else if(ulbin[i]==0x70 && ulbin[i+1]==0x41){
					
				}else if(ulbin[i]==0x20 && ulbin[i+1]==0x41){
					i+=6;
					readFlag=3;
					leng3=(ulbin[i+1]<<8|ulbin[i]);
					coord=new int[3*leng3];
					i+=2;
					j=0;
					k=0;
				}
			}
			if(readFlag==3){
				//读取面信息列表
				coord[j*3]=ulbin[i+1]<<8|ulbin[i];
				coord[j*3+1]=ulbin[i+5]<<8|ulbin[i+4];
				coord[j*3+2]=ulbin[i+3]<<8|ulbin[i+2];
				j++;
				if(j>=leng3){
					break;
				}
				i+=7;
			}
			if(readFlag==4){
				floatbin[0]=ulbin[i];
				floatbin[1]=ulbin[i+1];
				floatbin[2]=ulbin[i+2];
				floatbin[3]=ulbin[i+3];
				uv[j][0]=BitConverter.ToSingle(floatbin,0);
				i+=4;
				floatbin[0]=ulbin[i];
				floatbin[1]=ulbin[i+1];
				floatbin[2]=ulbin[i+2];
				floatbin[3]=ulbin[i+3];
				uv[j][1]=1-BitConverter.ToSingle(floatbin,0);
				j++;
				if(j>=leng2){
					readFlag=2;
				}
				i+=3;
			}
		}

		MeshFilter mf=obj.GetComponent<MeshFilter>()!=null?obj.GetComponent<MeshFilter>():obj.AddComponent<MeshFilter>();
		MeshRenderer mr=obj.GetComponent<MeshRenderer>()!=null?obj.GetComponent<MeshRenderer>():obj.AddComponent<MeshRenderer>();
		mr.material = new Material(Shader.Find(shaderName));
		mr.material.color = new Color(0.5f, 0.5f, 0.5f);
		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

		mf.mesh.vertices=point;
		if(uv.Length>0 && !forceAutoUV && !MeshUtil.sameUV(uv)) {
			mf.mesh.uv=uv;
		} else {
			MeshUtil.autoUV(mf.mesh);
		}
		mf.mesh.RecalculateBounds();
		MeshUtil.splitVerticesFromAngle(mf.mesh,angleLimit,false,coord);
		//MeshUtil.autoUV(mf.mesh);
		MeshUtil.splitMeshFromLimit(obj,coord);
		Bounds bounds=mf.mesh.bounds;
		pointMax=Mathf.Max(bounds.max.x-bounds.min.x,bounds.max.y-bounds.min.y,bounds.max.z-bounds.min.z);

		float scale=7.5f/pointMax/Mathf.Sqrt(Screen.dpi/96.0f);
		obj.transform.localScale=new Vector3(scale,scale,scale);
		if(moveToCenter){
			obj.transform.localPosition=new Vector3(-(bounds.min.x+bounds.max.x)*scale*0.5f,-(bounds.min.y+bounds.max.y)*scale*0.5f,-(bounds.min.z+bounds.max.z)*scale*0.5f);
		}
	}
	
	
	
    /// <summary>
    /// 加载obj文件
    /// </summary>
	void loadObjFile(byte[] ulbin,GameObject obj,float angleLimit=30f,bool moveToCenter=true,bool forceAutoUV=false){
		int leng=ulbin.Length;//文件的字节数
		
		string str0=System.Text.Encoding.UTF8.GetString(ulbin);
		
		string[] arr=str0.Split(new char[]{'\n'});
		if(arr.Length==0){
			arr=str0.Split(new char[]{'\r'});
		}
		List<Vector3> point=new List<Vector3>();
		List<int> coord=new List<int>();
		List<int> coord2=new List<int>();
		List<Vector2> uv=new List<Vector2>();
		int i;
		int j;
		int k;
		leng=arr.Length;
		string[] arrt;
		string[] arrt2;
		List<int> coordi=new List<int>();
		List<int> coord2i=new List<int>();
		float temp;
		
		List<Vector3> pointn=new List<Vector3>();
		List<int> nindex=new List<int>();
		List<int> coordni=new List<int>();
		try{
			for(i=0;i<leng;i++){
				if(arr[i].Length<3){
					continue;
				}
				if(arr[i][0]=='v' && arr[i][1]==' '){
					arrt=arr[i].Split(new char[]{' '});
					if(arrt.Length>=4){
						point.Add(new Vector3(Convert.ToSingle(arrt[1]),Convert.ToSingle(arrt[2]),-Convert.ToSingle(arrt[3])));
					}
				}
				if(arr[i][0]=='v' && arr[i][1]=='t' && arr[i][2]==' '){
					arrt=arr[i].Split(new char[]{' '});
					if(arrt.Length>=3){
						uv.Add(new Vector2(Convert.ToSingle(arrt[1]),1-Convert.ToSingle(arrt[2])));
					}
				}
				if(arr[i][0]=='v' && arr[i][1]=='n' && arr[i][2]==' '){
					arrt=arr[i].Split(new char[]{' '});
					if(arrt.Length>=4){
						//pointn.push([Number(arrt[1]),-Number(arrt[3]),Number(arrt[2])]);	
						pointn.Add(new Vector3(Convert.ToSingle(arrt[1]),Convert.ToSingle(arrt[2]),-Convert.ToSingle(arrt[3])));
					}
				}
				if(arr[i][0]=='f' && arr[i][1]==' '){
					arrt=arr[i].Split(new char[]{' '});
					if(arrt.Length<3){
						continue;
					}
					coordi=new List<int>();
					coord2i=new List<int>();
					coordni=new List<int>();
				
					for(j=1;j<arrt.Length;j++){
						arrt2=arrt[j].Split(new char[]{'/'});
						if(arrt2.Length>=1){
							coordi.Add(Convert.ToInt32(arrt2[0])-1);
						}
						if(arrt2.Length>=2){
							coord2i.Add(Convert.ToInt32(arrt2[1])-1);
						}
						if(arrt2.Length>=3){
							coordni.Add(Convert.ToInt32(arrt2[2])-1);
						}
					}
					
					for(k=coordi.Count-1;k>=2;k--){
						coord.Add(coordi[0]);
						coord.Add(coordi[k]);
						coord.Add(coordi[k-1]);
					}
					for(k=coord2i.Count-1;k>=2;k--){
						coord2.Add(coord2i[0]);
						coord2.Add(coord2i[k]);
						coord2.Add(coord2i[k-1]);
					}
					for(k=coordni.Count-1;k>=2;k--){
						nindex.Add(coordni[0]);
						nindex.Add(coordni[k]);
						nindex.Add(coordni[k-1]);
					}
				}
			}
		}catch(Exception ex){
			Debug.Log(ex);
			return;
		}

		MeshFilter mf=obj.GetComponent<MeshFilter>()!=null?obj.GetComponent<MeshFilter>():obj.AddComponent<MeshFilter>();
		MeshRenderer mr=obj.GetComponent<MeshRenderer>()!=null?obj.GetComponent<MeshRenderer>():obj.AddComponent<MeshRenderer>();
		mr.material = new Material(Shader.Find(shaderName));
		mr.material.color = new Color(0.5f, 0.5f, 0.5f);
		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
		
		int[] coordArr=coord.ToArray();
		mf.mesh.vertices=point.ToArray();
		if(nindex.Count>0 && pointn.Count>0) {
			MeshUtil.attachExternalNormalData(mf.mesh,pointn.ToArray(),nindex.ToArray(),coordArr);
		} else {
			MeshUtil.splitVerticesFromAngle(mf.mesh,angleLimit,false,coordArr);
		}
		
		if(uv.Count>0 && !forceAutoUV) {
			if(coord2.Count>0) {
				MeshUtil.splitVerticesFromUv(mf.mesh,uv.ToArray(),coord2.ToArray(),coordArr);
			} else {
				mf.mesh.uv=uv.ToArray();
			}
		} else {
			MeshUtil.autoUV(mf.mesh);
		}
		mf.mesh.RecalculateBounds();
		MeshUtil.splitMeshFromLimit(obj,coordArr);
		Bounds bounds=mf.mesh.bounds;
		float pointMax=Mathf.Max(bounds.max.x-bounds.min.x,bounds.max.y-bounds.min.y,bounds.max.z-bounds.min.z);
		float scale=10/pointMax/Mathf.Sqrt(Screen.dpi/96.0f);
		obj.transform.localScale=new Vector3(scale,scale,scale);
		if(moveToCenter){
			obj.transform.localPosition=new Vector3(-(bounds.min.x+bounds.max.x)*scale*0.5f,-(bounds.min.y+bounds.max.y)*scale*0.5f,-(bounds.min.z+bounds.max.z)*scale*0.5f);
		}
	}
	
	void loadVrmlFile(string fileStr0,byte[] ulstr0,GameObject v0,float defaultAngleLimit=30f,bool moveToCenter=true,bool forceAutoUV=false,SetCameraHandler onSetCamera=null,bool isInline=false){
		
		List<string> stackType=new List<string>();//按大括号的关系构建的类型堆栈

		GameObject vt=v0;
		
		float temp;//用于交换的临时变量
		int i=0;int j=0;int k=0;int l=0;int m=0;//循环变量
		bool isComment=false;//是否为注释
		bool isLabel=false;//是否为标识符
		string str;
		string str2;
		float[] infoArr=new float[]{0,0,0,0};
		bool ccw=true;
		Dictionary<string,Vector3[]> definedPoint=new Dictionary<string,Vector3[]>();
		Dictionary<string,Vector2[]> definedUv=new Dictionary<string,Vector2[]>();
		string definedGeometryKey="";
		
		Dictionary<string,Mesh> definedMesh=new Dictionary<string,Mesh>();
		Dictionary<string,int[]> definedTriangles=new Dictionary<string,int[]>();
		Dictionary<string,bool> definedExceeded=new Dictionary<string,bool>();
		Dictionary<string,Material> urlToMaterial=new Dictionary<string,Material>();
		float alpha = 1;
		bool useSharedMesh = false;
		bool isPreset=false;
		int shapeCount=0;
		int shapeCountInTransform=0;
		bool hasCamera=false;
		bool hasInlineRef=false;
		Vector2[] uv=new Vector2[0];
		int[] coord=new int[0];
		int[] coord2=new int[0];
		float rgeo=float.NaN;
		float hgeo=float.NaN;
		float[] uvinfo=new float[]{0,0,1,1,0,0,0};
		float[] uvMatrix=null;
		List<GameObject> inlineModelArray=new List<GameObject>();
		List<string> inlineUrlArray=new List<string>();
		byte ulstri=0;
		string lastTransfromName="";
		float angleLimit=defaultAngleLimit;
		byte ulstr0i=0;
		j=0;
		int leng0=ulstr0.Length;//vrml总文件的字符数
		int leng;//数组的长度
		byte[] ulstr=new byte[ulstr0.Length];
		Vector3 cameraPosition=new Vector3(0,0,0);
		Vector3 cameraEulerAngles=new Vector3(0,0,0);
		float cameraFieldOfView=60f;
		for(i=0;i<leng0;i++){
			ulstr0i=ulstr0[i];
			if(ulstr0i=='\r' || ulstr0i=='\n'){
				if(isComment){
					j=i;
				}
				isComment=false;
			}
			if(ulstr0i=='$'){
				isComment=true;
			}
			if(isComment) {
				ulstr[i]=(byte)' ';
			} else {
				ulstr[i]=ulstr0[i];
			}
		}
		ulstr0=ulstr;
		for(i=0;i<leng0;i++){
			ulstri=ulstr0[i];
			if(ulstri=='{'){
				stackType.Add(getPrevWord(ulstr0,i-1));
				str=stackType[stackType.Count-1];
				if(str=="Shape") {
					shapeCount++;
				}
				if(str=="Inline") {
					hasInlineRef=true;
				}
				if(str=="Viewpoint") {
					hasCamera=true;
				}
			}
			if(ulstri=='}' && stackType.Count>0){
				stackType.RemoveAt(stackType.Count-1);
			}
		}
		stackType=new List<string>();
		j=0;
		isComment=false;

		for(i=0;i<leng0;i++){
			ulstri=ulstr0[i];
			if(ulstri=='{'){
				stackType.Add(getPrevWord(ulstr0,i-1));
				str=stackType[stackType.Count-1];
				if(str=="Transform" || str=="Shape" || str=="Inline"){
					if(str=="Transform" && getPrevWord(ulstr0,i-1,3)=="DEF"){
						lastTransfromName=getPrevWord(ulstr0,i-1,2);
						shapeCountInTransform=0;
					}else if(str=="Transform"){
						lastTransfromName="";
						shapeCountInTransform=0;
					}
					if(str=="Shape"){
						shapeCountInTransform++;
					}
					if(str!="Shape" || shapeCountInTransform>1) {
						if(shapeCount>1 || hasCamera || hasInlineRef || isInline) {
							GameObject vtp=vt;
							vt=new GameObject();
							vt.transform.SetParent(vtp.transform);
						}
					
						vt.transform.localPosition=new Vector3(0,0,0);
						vt.transform.localEulerAngles=new Vector3(0,0,0);
						vt.transform.localScale=new Vector3(1,1,1);
					}
					
					if(vt.GetComponent<MeshFilter>()==null) {
						vt.AddComponent<MeshFilter>();
					}
					if(vt.GetComponent<MeshRenderer>()==null) {
						vt.AddComponent<MeshRenderer>();
					}
					vt.GetComponent<MeshRenderer>().material=new Material(Shader.Find(shaderName));

					
					if(lastTransfromName!="" && str!="Inline"){
						vt.name=lastTransfromName;
					}
					if(str=="Inline"){
						inlineModelArray.Add(vt);
						inlineUrlArray.Add("");
					}
					ccw=true;
					coord=new int[0];
					coord2=new int[0];
					uv=new Vector2[0];
					rgeo=float.NaN;
					hgeo=float.NaN;
					uvinfo=new float[]{0,0,1,1,0,0,0};
					uvMatrix=null;
					isPreset=false;
					definedGeometryKey="";
					angleLimit=defaultAngleLimit;
				}
			}
			
			if(ulstri>='A' && ulstri<='Z' || ulstri>='a' && ulstri<='z' || ulstri=='_'){
				if(!isLabel){
					j=i;
				}
				isLabel=true;
			}else if(isLabel && stackType.Count>0){
				isLabel=false;
				str=System.Text.Encoding.UTF8.GetString(ulstr0,j,i-j);
				if(str=="position" && stackType[stackType.Count-1]=="Viewpoint"){
					getNumber(ulstr0,infoArr,3,i);
					cameraPosition=new Vector3(infoArr[0],infoArr[1],-infoArr[2]);
				}
				if(str=="orientation" && stackType[stackType.Count-1]=="Viewpoint"){
					infoArr[2]=1;
					getNumber(ulstr0,infoArr,4,i);
					cameraEulerAngles=axisRotationToEularAngles(infoArr[0],infoArr[1],-infoArr[2],infoArr[3]);
				}
				if(str=="fieldOfView" && stackType[stackType.Count-1]=="Viewpoint"){
					getNumber(ulstr0,infoArr,1,i);
					cameraFieldOfView=infoArr[0]*90/Mathf.PI;
				}
				if(str=="translation" && stackType[stackType.Count-1]=="Transform"){
					getNumber(ulstr0,infoArr,3,i);
					vt.transform.localPosition=new Vector3(infoArr[0],infoArr[1],-infoArr[2]);
				}
				if(str=="rotation" && stackType[stackType.Count-1]=="Transform"){
					infoArr[2]=1;
					getNumber(ulstr0,infoArr,4,i);
					vt.transform.localEulerAngles=axisRotationToEularAngles(infoArr[0],infoArr[1],-infoArr[2],infoArr[3]);
				}
				if(str=="scale" && stackType[stackType.Count-1]=="Transform"){
					infoArr[0]=1;
					infoArr[1]=1;
					infoArr[2]=1;
					getNumber(ulstr0,infoArr,3,i);
					vt.transform.localScale=new Vector3(infoArr[0],infoArr[1],infoArr[2]);
				}
				if(str=="center" && stackType[stackType.Count-1]=="TextureTransform"){
					getNumber(ulstr0,infoArr,2,i);
					uvinfo[0]=infoArr[0];
					uvinfo[1]=infoArr[1];
					if(uvMatrix==null) {
						uvMatrix=new float[]{1,0,0,1,0,0};
					}
				}
				if(str=="scale" && stackType[stackType.Count-1]=="TextureTransform"){
					getNumber(ulstr0,infoArr,3,i);
					uvinfo[2]=infoArr[0];
					uvinfo[3]=infoArr[1];
					if(uvMatrix==null) {
						uvMatrix=new float[]{1,0,0,1,0,0};
					}
				}
				if(str=="rotation" && stackType[stackType.Count-1]=="TextureTransform"){
					getNumber(ulstr0,infoArr,1,i);
					uvinfo[4]=infoArr[0];
					if(uvMatrix==null) {
						uvMatrix=new float[]{1,0,0,1,0,0};
					}
				}
				if(str=="translation" && stackType[stackType.Count-1]=="TextureTransform"){
					getNumber(ulstr0,infoArr,2,i);
					uvinfo[5]=infoArr[0];
					uvinfo[6]=infoArr[1];
					if(uvMatrix==null) {
						uvMatrix=new float[]{1,0,0,1,0,0};
					}
				}
				if(str=="diffuseColor" && stackType[stackType.Count-1]=="Material"){
					getNumber(ulstr0,infoArr,3,i);
					MeshRenderer mr=vt.GetComponent<MeshRenderer>()!=null?vt.GetComponent<MeshRenderer>():vt.AddComponent<MeshRenderer>();
					if(mr.material==null) {
						mr.material=new Material(Shader.Find(shaderName));
					}
					mr.material.color=new Color(infoArr[0],infoArr[1],infoArr[2]);
					//vt.ambientr=infoArr[0]*127;
					//vt.ambientg=infoArr[1]*127;
					//vt.ambientb=infoArr[2]*127;
				}
				if(str=="shininess" && stackType[stackType.Count-1]=="Material"){
					getNumber(ulstr0,infoArr,1,i);
					MeshRenderer mr=vt.GetComponent<MeshRenderer>()!=null?vt.GetComponent<MeshRenderer>():vt.AddComponent<MeshRenderer>();
					if(mr.material==null) {
						mr.material=new Material(Shader.Find(shaderName));
					}
					//mr.material.
				}
				if(str=="transparency" && stackType[stackType.Count-1]=="Material"){
					getNumber(ulstr0,infoArr,1,i);
					MeshRenderer mr=vt.GetComponent<MeshRenderer>()!=null?vt.GetComponent<MeshRenderer>():vt.AddComponent<MeshRenderer>();
					if(mr.material==null) {
						mr.material=new Material(Shader.Find(shaderName));
					}
					alpha=1f-infoArr[0];
					if(alpha<=0.01f) {
						vt.SetActive(false);
						alpha = 0;
					} else if (alpha<=0.99f && transparentMaterialSample!=null)
					{
						Material mat0 = mr.material;
						Material mat=new Material(transparentMaterialSample);
						Color c = mat0.color;
						c.a = alpha;
						mat.color = c;
						mat.mainTexture = mat0.mainTexture;
						mr.material = mat;
					}
					else
					{
						alpha = 1;
					}
					
				}
				
				if(str=="url" && stackType[stackType.Count-1]=="ImageTexture"){
					//k=ulstr.indexOf("\"",i);
					//l=ulstr.indexOf("\"",k+1);
					for(k=i;k<leng0;k++){
						if(ulstr0[k]=='"'){
							break;
						}
					}
					for(l=k+1;l<leng0;l++){
						if(ulstr0[l]==34){
							break;
						}
					}

					str2=getAbsolutePath(System.Text.Encoding.UTF8.GetString(ulstr0,k+1,l-k-1),fileStr0);
					if(!File.Exists(str2) && System.Text.Encoding.GetEncoding("gbk")!=null) {
						str2=getAbsolutePath(System.Text.Encoding.GetEncoding("gbk").GetString(ulstr0,k+1,l-k-1),fileStr0);
					}
					loadBitmapTextureToObj(str2,vt,urlToMaterial,alpha);
				}
				if(str=="url" && stackType[stackType.Count-1]=="Inline"){
					//k=ulstr.indexOf("\"",i);
					//l=ulstr.indexOf("\"",k+1);
					for(k=i;k<leng0;k++){
						if(ulstr0[k]=='"'){
							break;
						}
					}
					for(l=k+1;l<leng0;l++){
						if(ulstr0[l]==34){
							break;
						}
					}
					str2=getAbsolutePath(System.Text.Encoding.UTF8.GetString(ulstr0,k+1,l-k-1),fileStr0);
					if(!File.Exists(str2) && System.Text.Encoding.GetEncoding("gbk")!=null) {
						str2=getAbsolutePath(System.Text.Encoding.GetEncoding("gbk").GetString(ulstr0,k+1,l-k-1),fileStr0);
					}
					if(inlineUrlArray.Count>0){
						inlineUrlArray[inlineUrlArray.Count-1]=str2;
					}
					string[] nameSeg=str2.Split(new char[]{'\\','/','.'});
					if(nameSeg.Length>=2) {
						vt.name=nameSeg[nameSeg.Length-2];
					}
				}
				if(str=="geometry" && stackType[stackType.Count-1]=="Shape" && getNextWord(ulstr0,i,1)=="DEF"){
					string key=getNextWord(ulstr0,i,2);
					definedGeometryKey=key;
					vt.name=key.Replace("-FACES","");
					useSharedMesh = false;
				}
				if(str=="geometry" && stackType[stackType.Count-1]=="Shape" && getNextWord(ulstr0,i,1)=="USE"){
					useSharedMesh = false;
					string key=getNextWord(ulstr0,i,2);
					
					if(definedMesh.ContainsKey(key) && definedTriangles.ContainsKey(key)){
						if (definedExceeded.ContainsKey(key) && definedExceeded[key] == true)
						{
							copyMeshToGameObject(vt,definedMesh[key],definedTriangles[key]);
						}
						else
						{
							MeshFilter mf=vt.GetComponent<MeshFilter>()!=null?vt.GetComponent<MeshFilter>():vt.AddComponent<MeshFilter>();
							mf.sharedMesh = definedMesh[key];
							useSharedMesh = true;
						}
					}
				}
				if(str=="ccw" && stackType[stackType.Count-1]=="IndexedFaceSet"){
					//ccw=ulstr.substring(i+1,i+6)!="FALSE";
					ccw=ulstr0[i+1]!='F';
				}
				/*if(str=="solid" && stackType[stackType.Count-1]=="IndexedFaceSet"){
					
				}*/
				if(str=="creaseAngle" && stackType[stackType.Count-1]=="IndexedFaceSet"){
					getNumber(ulstr0,infoArr,1,i);
					angleLimit=infoArr[0]*180/Mathf.PI;
				}
				if(str=="point" && stackType[stackType.Count-1]=="Coordinate"){
					useSharedMesh = false;
					str2="";
					if(getPrevWord(ulstr0,i,4)=="DEF"){
						str2=getPrevWord(ulstr0,i,3);
					}
					
					List<List<float>> point0=new List<List<float>>();
					i=getNumberTable(ulstr0,point0,3,false,i);
					leng=point0.Count;
					Vector3[] point=new Vector3[leng];

					for(l=0;l<leng;l++){
						if(point0[l].Count>=3) {
							point[l]=new Vector3(point0[l][0],point0[l][1],-point0[l][2]);
						}
					}
					MeshFilter mf=vt.GetComponent<MeshFilter>()!=null?vt.GetComponent<MeshFilter>():vt.AddComponent<MeshFilter>();
					mf.mesh.vertices=point;
					
					if(str2!=""){
						if(!definedPoint.ContainsKey(str2)) {
							definedPoint.Add(str2,(Vector3[])point.Clone());
						} else {
							definedPoint[str2]=(Vector3[])point.Clone();
						}
					}
				}
				if(str=="coord" && stackType[stackType.Count-1]=="IndexedFaceSet" && getNextWord(ulstr0,i,1)=="USE"){
					useSharedMesh = false;
					str2=getNextWord(ulstr0,i,2);
					MeshFilter mf=vt.GetComponent<MeshFilter>()!=null?vt.GetComponent<MeshFilter>():vt.AddComponent<MeshFilter>();
					if(definedPoint.ContainsKey(str2)){
						mf.mesh.vertices=(Vector3[])definedPoint[str2].Clone();
					}
				}
				if(str=="coordIndex" && stackType[stackType.Count-1]=="IndexedFaceSet"){
					useSharedMesh = false;
					List<List<float>> coord0=new List<List<float>>();
					i=getNumberTable(ulstr0,coord0,100,true,i);
					leng=coord0.Count;
					List<int> coordList=new List<int>();
					for(l=0;l<leng;l++){
						for(m=coord0[l].Count-1;m>=2;m--){
							coordList.Add((int)coord0[l][0]);
							coordList.Add(ccw?(int)coord0[l][m]:(int)coord0[l][m-1]);
							coordList.Add(ccw?(int)coord0[l][m-1]:(int)coord0[l][m]);
						}
					}
					coord=coordList.ToArray();
				}
				if(str=="point" && stackType[stackType.Count-1]=="TextureCoordinate"){
					useSharedMesh = false;
					str2="";
					if(getPrevWord(ulstr0,i,4)=="DEF"){
						str2=getPrevWord(ulstr0,i,3);
					}
					
					if(uvMatrix!=null && uvMatrix.Length>=6 && uvinfo.Length>=7){
						float tx0=uvinfo[5]-uvinfo[0];
						float ty0=uvinfo[6]-uvinfo[1];
						float cost=Mathf.Cos(uvinfo[4]);
						float sint=Mathf.Sin(uvinfo[4]);
						uvMatrix[0]=uvinfo[2]*cost;
						uvMatrix[1]=uvinfo[3]*sint;
						uvMatrix[2]=-uvinfo[2]*sint;
						uvMatrix[3]=uvinfo[3]*cost;
						uvMatrix[4]=tx0*uvMatrix[0]+ty0*uvMatrix[2]+uvinfo[0];
						uvMatrix[5]=tx0*uvMatrix[1]+ty0*uvMatrix[3]+uvinfo[1];
					}
					List<List<float>> uv0=new List<List<float>>();
					i=getNumberTable(ulstr0,uv0,2,false,i);
					leng=uv0.Count;
					uv=new Vector2[leng];
					for(l=0;l<leng;l++){
						if(uv0[l].Count>=2) {
							if(uvMatrix!=null && uvMatrix.Length>=6){
								uv[l]=new Vector2(uv0[l][0]*uvMatrix[0]+uv0[l][1]*uvMatrix[2]+uvMatrix[4],(uv0[l][0]*uvMatrix[1]+uv0[l][1]*uvMatrix[3]+uvMatrix[5]));
							} else {
								uv[l]=new Vector2(uv0[l][0],uv0[l][1]);
							}
						}
					}
					if(str2!=""){
						if(!definedUv.ContainsKey(str2)) {
							definedUv.Add(str2,(Vector2[])uv.Clone());
						} else {
							definedUv[str2]=(Vector2[])uv.Clone();
						}
					}
				}
				if(str=="texCoord" && stackType[stackType.Count-1]=="IndexedFaceSet" && getNextWord(ulstr0,i,1)=="USE"){
					useSharedMesh = false;
					str2=getNextWord(ulstr0,i,2);
					MeshFilter mf=vt.GetComponent<MeshFilter>()!=null?vt.GetComponent<MeshFilter>():vt.AddComponent<MeshFilter>();
					if(definedUv.ContainsKey(str2)){
						uv=(Vector2[])definedUv[str2].Clone();
					}
				}
				if(str=="texCoordIndex" && stackType[stackType.Count-1]=="IndexedFaceSet"){
					useSharedMesh = false;
					List<List<float>> coord0=new List<List<float>>();
					i=getNumberTable(ulstr0,coord0,100,true,i);
					leng=coord0.Count;
					List<int> coordList=new List<int>();
					for(l=0;l<leng;l++){
						for(m=coord0[l].Count-1;m>=2;m--){
							coordList.Add((int)coord0[l][0]);
							coordList.Add(ccw?(int)coord0[l][m]:(int)coord0[l][m-1]);
							coordList.Add(ccw?(int)coord0[l][m-1]:(int)coord0[l][m]);
						}
					}
					coord2=coordList.ToArray();
				}
				if(str=="size" && stackType[stackType.Count-1]=="Box"){
					getNumber(ulstr0,infoArr,3,i);
					GameObject vgeo=Model3d.Cuboid(infoArr[0],infoArr[1],infoArr[2]);
					copyMeshToGameObject(vt,vgeo.GetComponent<MeshFilter>().mesh);
					UnityEngine.Object.Destroy(vgeo);
					isPreset=true;
				}
				if(str=="radius" && stackType[stackType.Count-1]=="Sphere"){
					getNumber(ulstr0,infoArr,1,i);
					GameObject vgeo=Model3d.Sphere(infoArr[0]);
					copyMeshToGameObject(vt,vgeo.GetComponent<MeshFilter>().mesh);
					UnityEngine.Object.Destroy(vgeo);
					isPreset=true;
				}
				if(str=="bottomRadius" && stackType[stackType.Count-1]=="Cone"){
					getNumber(ulstr0,infoArr,1,i);
					rgeo=infoArr[0];
					if(!float.IsNaN(rgeo) && !float.IsNaN(hgeo)){
						GameObject vgeo=Model3d.Cone(rgeo,hgeo);
						copyMeshToGameObject(vt,vgeo.GetComponent<MeshFilter>().mesh);
						UnityEngine.Object.Destroy(vgeo);
						isPreset=true;
					}
				}
				if(str=="height" && stackType[stackType.Count-1]=="Cone"){
					getNumber(ulstr0,infoArr,1,i);
					hgeo=infoArr[0];
					if(!float.IsNaN(rgeo) && !float.IsNaN(hgeo)){
						GameObject vgeo=Model3d.Cone(rgeo,hgeo);
						copyMeshToGameObject(vt,vgeo.GetComponent<MeshFilter>().mesh);
						UnityEngine.Object.Destroy(vgeo);
						isPreset=true;
					}
				}
				if(str=="height" && stackType[stackType.Count-1]=="Cylinder"){
					getNumber(ulstr0,infoArr,1,i);
					rgeo=infoArr[0];
					if(!float.IsNaN(rgeo) && !float.IsNaN(hgeo)){
						GameObject vgeo=Model3d.Cylinder(rgeo,hgeo);
						copyMeshToGameObject(vt,vgeo.GetComponent<MeshFilter>().mesh);
						UnityEngine.Object.Destroy(vgeo);
						isPreset=true;
					}
				}
				if(str=="height" && stackType[stackType.Count-1]=="Cylinder"){
					getNumber(ulstr0,infoArr,1,i);
					hgeo=infoArr[0];
					if(!float.IsNaN(rgeo) && !float.IsNaN(hgeo)){
						GameObject vgeo=Model3d.Cylinder(rgeo,hgeo);
						copyMeshToGameObject(vt,vgeo.GetComponent<MeshFilter>().mesh);
						UnityEngine.Object.Destroy(vgeo);
						isPreset=true;
					}
				}
			}else if(isLabel){
				isLabel=false;
			}
			if(ulstri=='}' && stackType.Count>0){
				if(stackType[stackType.Count-1]=="Shape" && vt!=null){
					MeshFilter mf=vt.GetComponent<MeshFilter>();
					if (useSharedMesh)
					{
						MeshRenderer mr=vt.GetComponent<MeshRenderer>();
						if(mr!=null) {
							mr.shadowCastingMode = alpha>=1?UnityEngine.Rendering.ShadowCastingMode.TwoSided:UnityEngine.Rendering.ShadowCastingMode.Off;
						}
					}
					else if(mf!=null && mf.mesh.vertexCount>0 && coord!=null && coord.Length>0) {
						if(uv==null || uv.Length<=0 || forceAutoUV){
							MeshUtil.autoUV(mf.mesh);
							MeshUtil.splitVerticesFromAngle(mf.mesh,angleLimit,false,coord);
						} else if(coord2!=null && coord2.Length>0){
							MeshUtil.splitVerticesFromAngle(mf.mesh,angleLimit,true,coord);
							MeshUtil.splitVerticesFromUv(mf.mesh,uv,coord2,coord);
						} else if(coord.Length>0){
							if(uv.Length==mf.mesh.vertices.Length) {
								mf.mesh.uv=uv;
							} else if(uv.Length>=mf.mesh.vertices.Length){
								MeshUtil.autoUV(mf.mesh);
							}
							MeshUtil.splitVerticesFromAngle(mf.mesh,angleLimit,false,coord);
						}
						mf.mesh.RecalculateBounds();
						if(definedGeometryKey!=null && definedGeometryKey!="")
						{
							Mesh mesh;
							bool exceeded = false;
							if (mf.mesh.vertexCount <= MeshUtil.verticeLimit)
							{
								mesh = mf.mesh;
							}
							else
							{
								exceeded = true;
								mesh=new Mesh();
								mesh.vertices=mf.mesh.vertices;
								mesh.normals=mf.mesh.normals;
								mesh.uv=mf.mesh.uv;
							}
							if(!definedMesh.ContainsKey(definedGeometryKey)) {
								definedMesh.Add(definedGeometryKey,mesh);
							} else {
								definedMesh[definedGeometryKey]=mesh;
							}
							if(!definedTriangles.ContainsKey(definedGeometryKey)) {
								definedTriangles.Add(definedGeometryKey,(int[])coord.Clone());
							} else {
								definedTriangles[definedGeometryKey]=(int[])coord.Clone();
							}
							if(!definedExceeded.ContainsKey(definedGeometryKey)) {
								definedExceeded.Add(definedGeometryKey,exceeded);
							} else {
								definedExceeded[definedGeometryKey]=exceeded;
							}
						}
						MeshRenderer mr=vt.GetComponent<MeshRenderer>();
						if(mr!=null) {
							mr.shadowCastingMode = alpha>=1?UnityEngine.Rendering.ShadowCastingMode.TwoSided:UnityEngine.Rendering.ShadowCastingMode.Off;
						}
						if(coord!=null && coord.Length>0) {
							MeshUtil.splitMeshFromLimit(vt,coord);
						}
					} else if(mf==null || mf.mesh.triangles.Length==0){
						if(vt.GetComponent<MeshFilter>()!=null) {
							UnityEngine.Object.Destroy(vt.GetComponent<MeshFilter>());
						}
						if(vt.GetComponent<MeshRenderer>()!=null) {
							UnityEngine.Object.Destroy(vt.GetComponent<MeshRenderer>());
						}
					}
					if(shapeCountInTransform>1 && vt!=v0 && vt.transform.parent!=null) {
						vt=vt.transform.parent.gameObject;
					}
				}else if((stackType[stackType.Count-1]=="Transform" || stackType[stackType.Count-1]=="Inline") && vt!=null){
					MeshFilter mf=vt.GetComponent<MeshFilter>();
					if(!(mf!=null && mf.mesh.vertexCount>0 && mf.mesh.triangles.Length>0)) {
						if(vt.GetComponent<MeshFilter>()!=null) {
							UnityEngine.Object.Destroy(vt.GetComponent<MeshFilter>());
						}
						if(vt.GetComponent<MeshRenderer>()!=null) {
							UnityEngine.Object.Destroy(vt.GetComponent<MeshRenderer>());
						}
					}
					if(vt!=v0 && vt.transform.parent!=null) {
						vt=vt.transform.parent.gameObject;
					}
				}
				stackType.RemoveAt(stackType.Count-1);
			}
		}
		if(!isInline) {
			if(hasCamera) {
				v0.transform.localPosition=new Vector3(0,0,0);
				v0.transform.localEulerAngles=new Vector3(0,0,0);
				v0.transform.localScale=new Vector3(1,1,1);
				onSetCamera?.Invoke(cameraPosition,cameraEulerAngles,cameraFieldOfView);
			}else if(shapeCount==1 && !hasInlineRef){
				Bounds bounds=v0.GetComponent<MeshFilter>().mesh.bounds;
				float pointMax=Mathf.Max(bounds.max.x-bounds.min.x,bounds.max.y-bounds.min.y,bounds.max.z-bounds.min.z);
				float scale=10/pointMax/Mathf.Sqrt(Screen.dpi/96.0f);
				v0.transform.localScale=new Vector3(scale,scale,scale);
				if(moveToCenter){
					v0.transform.localPosition=new Vector3(-(bounds.min.x+bounds.max.x)*scale*0.5f,-(bounds.min.y+bounds.max.y)*scale*0.5f,-(bounds.min.z+bounds.max.z)*scale*0.5f);
				}
			}else{
				if(moveToCenter){
					v0.transform.localPosition=new Vector3(0,0,0);
					v0.transform.localEulerAngles=new Vector3(0,0,0);
				}
				v0.transform.localScale=new Vector3(1,1,1);
				Bounds bounds=getBounds(v0,true);
				float pointMax=Mathf.Max(bounds.max.x-bounds.min.x,bounds.max.y-bounds.min.y,bounds.max.z-bounds.min.z);
				float scale=10/pointMax/Mathf.Sqrt(Screen.dpi/96.0f);
				v0.transform.localScale=new Vector3(scale,scale,scale);
				if(moveToCenter){
					v0.transform.localPosition=new Vector3(-(bounds.min.x+bounds.max.x)*scale*0.5f,-(bounds.min.y+bounds.max.y)*scale*0.5f,-(bounds.min.z+bounds.max.z)*scale*0.5f);
				}
			}
		}
		if(inlineModelArray.Count>=0){
			for(i=0;i<inlineModelArray.Count && i<inlineUrlArray.Count;i++){
				if(inlineUrlArray[i]!=null && inlineUrlArray[i]!=""){
					loadVrmlFile(inlineUrlArray[i],readFile(inlineUrlArray[i]),inlineModelArray[i],defaultAngleLimit,false,false,null,true);
				}
			}
		}
	}

	
	
	string getPrevWord(byte[] ulstr,int i0,int wordNumber=1){
		int i=0;
		int j=0;
		bool readBegin=false;
		bool readBeginPrev=false;
		int count=0;
		int ulstri=0;
		j=i0;
		for(i=i0;i>=0;i--){
			ulstri=ulstr[i];
			readBeginPrev=readBegin;
			if((ulstri>='A' && ulstri<='Z') || (ulstri>='a' && ulstri<='z') || (ulstri>='0' && ulstri<='9') || ulstri=='_' || ulstri=='-' || ulstri=='+' || ulstri=='.' || ulstri==':' || ulstri>=128){
				if(!readBegin){
					j=i;
				}
				readBegin=true;
			}else if(readBegin){
				readBegin=false;
				if(readBeginPrev) {
					count++;
					if(count>=wordNumber){
						break;
					}
				}
			}
		}
		i++;
		j++;
		if(j>=ulstr.Length){
			j=ulstr.Length;
		}
		return System.Text.Encoding.UTF8.GetString(ulstr,i,j-i);
	}
	
	string getNextWord(byte[] ulstr,int i0,int wordNumber=1){
		int i=0;
		int j=0;
		bool readBegin=false;
		bool readBeginPrev=false;
		int count=0;
		int ulstri=0;
		j=i0;
		int leng0=ulstr.Length;
		for(i=i0;i<leng0;i++){
			ulstri=ulstr[i];
			readBeginPrev=readBegin;
			if((ulstri>='A' && ulstri<='Z') || (ulstri>='a' && ulstri<='z') || (ulstri>='0' && ulstri<='9') || ulstri=='_' || ulstri=='-' || ulstri=='+' || ulstri=='.' || ulstri==':' || ulstri>=128){
				if(!readBegin){
					j=i;
				}
				readBegin=true;
			}else if(readBegin){
				readBegin=false;
				if(readBeginPrev) {
					count++;
					if(count>=wordNumber){
						break;
					}
				}
			}
		}
		return System.Text.Encoding.UTF8.GetString(ulstr,j,i-j);
	}
	
	int getNumber(byte[] ulstr,float[] nums,int numCount,int i0){
		int i=0;
		int j=0;
		int k=0;
		bool readBegin=false;
		bool readBeginPrev=false;
		int ulstri=0;
		j=i0;
		int leng0=ulstr.Length;
		for(i=i0;i<leng0;i++){
			ulstri=ulstr[i];
			readBeginPrev=readBegin;
			if((ulstri>='0' && ulstri<='9') || ulstri=='E' || ulstri=='e' || ulstri=='-' || ulstri=='+' || ulstri=='.'){
				if(!readBegin){
					j=i;
				}
				readBegin=true;
			}else if(readBegin){
				readBegin=false;
				if(readBeginPrev) {
					float num=0f;
					if(k<nums.Length && float.TryParse(System.Text.Encoding.UTF8.GetString(ulstr,j,i-j),out num)){
						nums[k]=num;
						k++;
					}
					if(k>=numCount || k>=nums.Length){
						break;
					}
				}
			}
		}
		return i;
	}
	
	int getNumberTable(byte[] ulstr,List<List<float>> nums,int columnCount,bool useSeparatorM1,int i0){
		int i=0;
		int j=0;
		bool readBegin=false;
		bool readBeginPrev=false;
		int ulstri=0;
		j=i0;
		int leng0=ulstr.Length;
		nums.Clear();
		for(i=i0;i<leng0;i++){
			ulstri=ulstr[i];
			readBeginPrev=readBegin;
			
			if((ulstri>='0' && ulstri<='9') || ulstri=='E' || ulstri=='e' || ulstri=='-' || ulstri=='+' || ulstri=='.'){
				if(!readBegin){
					j=i;
				}
				readBegin=true;
			}else if(readBegin){
				readBegin=false;
				if(readBeginPrev) {
					string str=System.Text.Encoding.UTF8.GetString(ulstr,j,i-j);
					if(useSeparatorM1 && str=="-1") {
						nums.Add(new List<float>());
					} else {
						float num=0f;
						float.TryParse(str,out num);
						if(nums.Count==0) {
							nums.Add(new List<float>());
						}
						nums[nums.Count-1].Add(num);
						if(columnCount>0 && nums[nums.Count-1].Count>=columnCount) {
							nums.Add(new List<float>());
						}
					}
				}
				
			}
			if(ulstri==']' || ulstri=='}') {
				break;
			}
		}
		if(nums.Count>0 && nums[nums.Count-1].Count==0) {
			nums.RemoveAt(nums.Count-1);
		}
		return i;
	}

	public static string getAbsolutePath(string relativePath,string filePathNameBase) {
		relativePath=relativePath.Replace('\\','/');
		filePathNameBase=filePathNameBase.Replace('\\','/');
		if(relativePath.IndexOf(":")<0 && relativePath.IndexOf("/")!=0){
			int pos=filePathNameBase.Length;
			while(relativePath.Substring(0,3)=="../" && pos>0){
				relativePath=relativePath.Substring(3);
				pos=filePathNameBase.LastIndexOf("/",pos)-1;
			}
			pos=filePathNameBase.LastIndexOf("/",pos);
			return filePathNameBase.Substring(0,pos+1)+relativePath;
		} else {
			return relativePath;
		}
	}

	public static Bounds getBounds(GameObject target, bool include_children = true)
    {
 
        Renderer[] mrs = target.gameObject.GetComponentsInChildren<Renderer>();
        Vector3 center = target.transform.position;
        Bounds bounds = new Bounds(center, Vector3.zero);
        if (include_children)
        {
            if (mrs.Length != 0)
            {
				bool isFirst=true;
                foreach (Renderer mr in mrs)
                {
					if(isFirst) {
						bounds=mr.bounds;
					} else {
						bounds.Encapsulate(mr.bounds);
					}
					isFirst=false;
                }
            }
		} else {
			Renderer rend = target.GetComponentInChildren<Renderer>();
			if (rend)
			{
				bounds = rend.bounds;
			}
		}
 
        return bounds;
 
    }
	
	public static Vector3 axisRotationToEularAngles(float x0,float y0,float z0,float w0){
		float d=Mathf.Sqrt(x0*x0+y0*y0+z0*z0);
		x0/=d;
		y0/=d;
		z0/=d;
		float cosw02=Mathf.Cos(w0/2);
		float sinw02=Mathf.Sin(-w0/2);
		float rx=x0*sinw02;
		float ry=y0*sinw02;
		float rz=z0*sinw02;
		float rw=cosw02;
		Quaternion q = new Quaternion(rx,ry,rz,rw);
		return q.eulerAngles;
	}

	public static void copyMeshToGameObject(GameObject obj,Mesh mesh,int[] triangles=null){
		MeshFilter mf=obj.GetComponent<MeshFilter>()!=null?obj.GetComponent<MeshFilter>():obj.AddComponent<MeshFilter>();
		if(obj.GetComponent<MeshRenderer>()==null) {
			MeshRenderer mr=obj.AddComponent<MeshRenderer>();
			mr.material = new Material(Shader.Find(shaderName));
			mr.material.color = new Color(0.5f, 0.5f, 0.5f);
		}
		mf.mesh.vertices=mesh.vertices;
		mf.mesh.uv=mesh.uv;
		mf.mesh.normals=mesh.normals;
		mf.mesh.RecalculateBounds();
		if(triangles!=null && triangles.Length>0) {
			MeshUtil.splitMeshFromLimit(obj,triangles);
		} else {
			MeshUtil.splitMeshFromLimit(obj,mesh.triangles);
		}
	}
	
	
}