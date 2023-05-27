using System;
using System.Collections.Generic;
using UnityEngine;


class MeshUtil
{
	/// <summary>
	/// 每个模型的最大顶点数
	/// </summary>
	public static readonly int verticeLimit=65535;

	/// <summary>
	/// 创建顶点法向量
	/// </summary>
	/// <param name="mesh">模型网格</param>
	/// <param name="externalTriangles">外部的顶点三角形索引数据，顶点索引值可超过65535</param>
    public static void createNormal(Mesh mesh,int[] externalTriangles=null)
    {
		mesh.normals=createNormalFromData(mesh.vertices, externalTriangles!=null?externalTriangles:mesh.triangles);
	}

	/// <summary>
	/// 从数组中创建法向量
	/// </summary>
	/// <param name="vertices">顶点</param>
	/// <param name="triangles">三角形（顶点索引值可≥65535）</param>
	/// <param name="createTriangleNormal">是否生成平面法向量(true),否则生成顶点法向量(false)</param>
	/// <returns></returns>
	public static Vector3[] createNormalFromData(Vector3[] vertices,int[] triangles,bool createTriangleNormal=false)
    {
		Vector3[] point = vertices;
		int[] coord = triangles;

		float vx; float vy; float vz; float vx2; float vy2; float vz2;
		int leng = point.Length;
		int leng2 = coord.Length/3;
		Vector3[] pointn = new Vector3[createTriangleNormal?0:leng];
		Vector3[] coordn = new Vector3[leng2];
		int i; int j;int pos;
		float d1;//距离
		if(!createTriangleNormal) {
			for (i = 0; i < leng; i++) {
				pointn[i].Set(0,0,0);
			}
		}
		
		for (i = 0; i < leng2; i++)
		{
			pos=i*3;
			if(coord[pos]<0 || coord[pos]>=leng || coord[pos+1]<0 || coord[pos+1]>=leng || coord[pos+2]<0 || coord[pos+2]>=leng) {
				coordn[i].x = 0;
				coordn[i].y = 0;
				coordn[i].z = 0;
				continue;
			}
			vx = point[coord[pos+1]][0] - point[coord[pos]][0];
			vy = point[coord[pos+1]][1] - point[coord[pos]][1];
			vz = point[coord[pos+1]][2] - point[coord[pos]][2];
			vx2 = point[coord[pos+2]][0] - point[coord[pos]][0];
			vy2 = point[coord[pos+2]][1] - point[coord[pos]][1];
			vz2 = point[coord[pos+2]][2] - point[coord[pos]][2];
			coordn[i].x = -vz*vy2 + vy*vz2;//生成法向量
			coordn[i].y = -vx*vz2 + vz*vx2;
			coordn[i].z = -vy*vx2 + vx*vy2;



			d1 = Mathf.Sqrt(coordn[i].x*coordn[i].x + coordn[i].y*coordn[i].y + coordn[i].z*coordn[i].z);


			if (Mathf.Abs(d1)>=0.00001)
			{
				coordn[i].x /= d1;//将平面法向量数乘成单位向量
				coordn[i].y /= d1;
				coordn[i].z /= d1;
			} else {
				coordn[i].x = 0;//将平面法向量数乘成单位向量
				coordn[i].y = 0;
				coordn[i].z = 0;
			}
			if(!createTriangleNormal)
			{
				j = coord[pos];//生成点法向量
				pointn[j][0] += coordn[i].x;
				pointn[j][1] += coordn[i].y;
				pointn[j][2] += coordn[i].z;
				j = coord[pos+1];
				pointn[j][0] += coordn[i].x;
				pointn[j][1] += coordn[i].y;
				pointn[j][2] += coordn[i].z;
				j = coord[pos+2];
				pointn[j][0] += coordn[i].x;
				pointn[j][1] += coordn[i].y;
				pointn[j][2] += coordn[i].z;
			}
			//trace(coord[i]);
		}

		if(!createTriangleNormal) 
		{
			for (i = 0; i < leng; i++)
			{
				d1 = Mathf.Sqrt(pointn[i][0]*pointn[i][0] + pointn[i][1]*pointn[i][1] + pointn[i][2]*pointn[i][2]);
				if (Mathf.Abs(d1)>=0.00001)
				{
					pointn[i][0] /= d1;
					pointn[i][1] /= d1;
					pointn[i][2] /= d1;
				} else {
					pointn[i][0] /= 0;
					pointn[i][1] /= 0;
					pointn[i][2] /= 0;
				}
				
			}
		}
		return createTriangleNormal ? coordn : pointn;
	}

	/// <summary>
	/// 附加外部顶点法向量
	/// </summary>
	/// <param name="mesh">模型（包含顶点数据）</param>
	/// <param name="pointnSrc">源顶点法向量值</param>
	/// <param name="nindex">源顶点法向量的独立三角形索引</param>
	/// <param name="externalTriangles">外部的顶点三角形索引数据，顶点索引值可超过65535</param>
	public static void attachExternalNormalData(Mesh mesh,Vector3[] pointnSrc,int[] nindex,int[] externalTriangles=null){
		if(mesh.vertices!=null && mesh.vertices.Length==pointnSrc.Length && (nindex==null || nindex.Length==0)) {
			mesh.normals=pointnSrc;
			return;
		}
		List<Vector3> point = new List<Vector3>(mesh.vertices);
		List<Vector3> pointn = new List<Vector3>(new Vector3[point.Count]);
		int[] coord = externalTriangles!=null?externalTriangles:mesh.triangles;

		int leng=point.Count<=pointn.Count?point.Count:pointn.Count;
		int leng2=coord.Length/3;
		int leng3=pointnSrc.Length;
		int i;int j;int pos;

		
		int[] pointnChecked = new int[leng];

		Vector3 p0;Vector3 n0;
		for(i=0;i<leng;i++){
			pointn[i]=new Vector3(0,0,0);
			pointnChecked[i]=0;
		}
		for(i=0;i<leng2;i++){
			pos=i*3;
			if(pos+2>=nindex.Length){
				break;
			}
			if(coord[pos]<0 || coord[pos]>=leng || coord[pos+1]<0 || coord[pos+1]>=leng || coord[pos+2]<0 || coord[pos+2]>=leng){
				coord[pos]=0;
				coord[pos+1]=0;
				coord[pos+2]=0;
			}
			if(nindex[pos]<0 || nindex[pos]>=leng3 || nindex[pos+1]<0 || nindex[pos+1]>=leng3 || nindex[pos+2]<0 || nindex[pos+2]>=leng3){
				nindex[pos]=0;
				nindex[pos+1]=0;
				nindex[pos+2]=0;
			}

			for(j=0; j<3; j++) {
				p0=point[coord[pos+j]];
				n0=pointnSrc[nindex[pos+j]];
				if(pointnChecked[coord[pos+j]]<=0) {
					pointn[coord[pos+j]]=new Vector3(n0.x,n0.y,n0.z);
					pointnChecked[coord[pos+j]]=1;
				}else{
					point.Add(new Vector3(p0.x,p0.y,p0.z));
					pointn.Add(new Vector3(n0.x,n0.y,n0.z));
					coord[pos+j]=point.Count-1;
				}
			}
		}
		mesh.vertices=point.ToArray();
		mesh.normals=pointn.ToArray();
		if(externalTriangles==null) {
			mesh.triangles=coord;
		}
	}

	/// <summary>
	/// 当面片夹角过大时，将共用顶点分成多个，然后分别计算法向量
	/// </summary>
	/// <param name="mesh">模型网格</param>
	/// <param name="angle0">夹角阈值</param>
	/// <param name="hasDifferentUvTriangle">存在uv时，是否有与顶点不同的三角形索引</param>
	/// <param name="externalTriangles">外部的顶点三角形索引数据，顶点索引值可超过65535</param>
	public static void splitVerticesFromAngle(Mesh mesh,float angle0=30.0f,bool hasDifferentUvTriangle=false,int[] externalTriangles=null){
		if(angle0>180){
			createNormal(mesh,externalTriangles);
			return;
		}
		int[] coord = externalTriangles!=null?externalTriangles:mesh.triangles;
		Vector3[] coordn=createNormalFromData(mesh.vertices,coord,true);
		List<Vector3> point = new List<Vector3>(mesh.vertices);
		List<Vector2> uv = !hasDifferentUvTriangle && mesh.uv!=null?new List<Vector2>(mesh.uv):null;
		
		int i;
		int j;
		int k;
		int l;
		int m;
		int n;
		int leng=point.Count;
		int leng2=coordn.Length;
		if(leng<=0 || leng2<=0){
			return;
		}
		float cos0=angle0>=0?Mathf.Cos(angle0*Mathf.PI/180):1;
					
		float cost;
		List<List<int>> pointci=new List<List<int>>(new List<int>[leng]);//点所在平面数组
		List<List<int>> pointcj=new List<List<int>>(new List<int>[leng]);//点所在平面的点序号数组
		List<int> jksign;//某个点的连续的平面组的编组序号
		bool ordered=false;//数组已排好序		
		int temp;
		for(i=0;i<leng;i++){
			pointci[i]=new List<int>();
			pointcj[i]=new List<int>();
		}
		int pos;
		for(i=0;i<leng2;i++){
			pos=i*3;
			if(coord[pos]<0 || coord[pos]>=leng || coord[pos+1]<0 || coord[pos+1]>=leng || coord[pos+2]<0 || coord[pos+2]>=leng) {
				continue;
			}
			pointci[coord[pos]].Add(i);
			pointcj[coord[pos]].Add(0);
			pointci[coord[pos+1]].Add(i);
			pointcj[coord[pos+1]].Add(1);
			pointci[coord[pos+2]].Add(i);
			pointcj[coord[pos+2]].Add(2);
		}
		l=leng-1;
		for(i=0;i<leng;i++){
			jksign=new List<int>(new int[pointci[i].Count]);
			if(jksign.Count==0) {
				continue;
			}
			for(j=0;j<jksign.Count;j++){
				jksign[j]=j;
			}
			for(j=0;j<jksign.Count;j++){
				for(k=j+1;k<jksign.Count;k++){
					if(jksign[k]==jksign[j]){
						continue;
					}
					m=pointci[i][j];
					n=pointci[i][k];
					cost=coordn[m][0]*coordn[n][0]+coordn[m][1]*coordn[n][1]+coordn[m][2]*coordn[n][2];
					if(cost>cos0){
						jksign[k]=jksign[j];
					}
				}
			}
			for(j=0;j<jksign.Count;j++){
				ordered=true;
				for(k=1;k<jksign.Count-j;k++){
					if(jksign[j]>jksign[k]){
						temp=jksign[k];
						jksign[k]=jksign[j];
						jksign[j]=temp;
						temp=pointci[i][k];
						pointci[i][k]=pointci[i][j];
						pointci[i][j]=temp;
						temp=pointcj[i][k];
						pointcj[i][k]=pointcj[i][j];
						pointcj[i][j]=temp;
						ordered=false;
					}
				}
				if(ordered){
					break;
				}
			}
			k=jksign[0];
			for(j=0;j<jksign.Count;j++){
				if(k==jksign[j]){
					if(k!=jksign[0]){
						coord[pointci[i][j]*3+pointcj[i][j]]=l;
					}
					continue;
				}
				k=jksign[j];
				point.Add(new Vector3(point[i][0],point[i][1],point[i][2]));
				if(uv!=null && uv.Count>0 && !hasDifferentUvTriangle){
					uv.Add(new Vector2(uv[i][0],uv[i][1]));
				}
				l++;
				coord[pointci[i][j]*3+pointcj[i][j]]=l;
			}
		}
		if(point.Count != mesh.vertices.Length) {
			mesh.vertices=point.ToArray();
		}
		if(uv!=null && uv.Count>0 && !hasDifferentUvTriangle) {
			mesh.uv=uv.ToArray();
		}
		if(externalTriangles==null) {
			mesh.triangles=coord;
		}
		createNormal(mesh,externalTriangles);
	}

	/// <summary>
	/// 当uv使用不同的三角形索引时，将共用顶点分成多个，来让uv和顶点用到相同的三角形索引
	/// </summary>
	/// <param name="mesh">模型网格</param>
	/// <param name="uvSrc">源uv值</param>
	/// <param name="uvTriangles">源uv的独立三角形索引</param>
	/// <param name="externalTriangles">外部的顶点三角形索引数据，顶点索引值可超过65535</param>
	public static void splitVerticesFromUv(Mesh mesh,Vector2[] uvSrc,int[] uvTriangles,int[] externalTriangles=null){
		if(mesh.vertices!=null && mesh.vertices.Length==uvSrc.Length && (uvTriangles==null || uvTriangles.Length==0)) {
			if(mesh.normals==null || mesh.normals.Length==0) {
				createNormal(mesh,externalTriangles);
			}
			mesh.uv=uvSrc;
			return;
		}
		List<Vector3> point = new List<Vector3>(mesh.vertices);
		List<Vector2> uv = new List<Vector2>(new Vector2[point.Count]);
		if(mesh.normals==null || mesh.normals.Length==0) {
			createNormal(mesh,externalTriangles);
		}
		List<Vector3> pointn = new List<Vector3>(mesh.normals);
		int[] coord = externalTriangles!=null?externalTriangles:mesh.triangles;
		int[] coord2 = uvTriangles;

		int leng=point.Count<=pointn.Count?point.Count:pointn.Count;
		int leng2=coord.Length/3;
		int leng3=uvSrc.Length;
		int i;int j;int pos;
		if(leng==0 || leng2==0 || leng3==0) {
			return;
		}

		
		int[] uvChecked = new int[leng];

		Vector3 p0;Vector3 n0;Vector2 uv0;
		for(i=0;i<leng;i++){
			uv[i]=new Vector3(0,0);
			uvChecked[i]=0;
		}
		for(i=0;i<leng2;i++){
			pos=i*3;
			if(pos+2>=coord2.Length){
				break;
			}
			if(coord[pos]<0 || coord[pos]>=leng || coord[pos+1]<0 || coord[pos+1]>=leng || coord[pos+2]<0 || coord[pos+2]>=leng){
				coord[pos]=0;
				coord[pos+1]=0;
				coord[pos+2]=0;
			}
			if(coord2[pos]<0 || coord2[pos]>=leng3 || coord2[pos+1]<0 || coord2[pos+1]>=leng3 || coord2[pos+2]<0 || coord2[pos+2]>=leng3){
				coord2[pos]=0;
				coord2[pos+1]=0;
				coord2[pos+2]=0;
			}

			for(j=0; j<3; j++) {
				p0=point[coord[pos+j]];
				n0=pointn[coord[pos+j]];
				uv0=uvSrc[coord2[pos+j]];
				if(uvChecked[coord[pos+j]]<=0) {
					uv[coord[pos+j]]=new Vector2(uv0.x,uv0.y);
					uvChecked[coord[pos+j]]=1;
				}else{
					point.Add(new Vector3(p0.x,p0.y,p0.z));
					pointn.Add(new Vector3(n0.x,n0.y,n0.z));
					uv.Add(new Vector2(uv0.x,uv0.y));
					coord[pos+j]=point.Count-1;
				}
			}
		}
		mesh.vertices=point.ToArray();
		mesh.uv=uv.ToArray();
		mesh.normals=pointn.ToArray();
		if(externalTriangles==null) {
			mesh.triangles=coord;
		}
	}

	/// <summary>
	/// 当顶点数超过限度时，将模型分成多个，放到子transform中，并共用一套材质
	/// </summary>
	/// <param name="obj">模型</param>
	/// <param name="externalTriangles">外部的顶点三角形索引数据，顶点索引值可超过限度</param>
	public static void splitMeshFromLimit(GameObject obj,int[] externalTriangles){
		MeshFilter mf0=obj.GetComponent<MeshFilter>();
		if(mf0==null) {
			return;
		}
		if(mf0.mesh.vertexCount<=verticeLimit) {
			mf0.mesh.triangles=externalTriangles;
			return;
		}
		Mesh mesh=mf0.mesh;
		int vl=mesh.vertexCount;
		int vm=verticeLimit;

		List<Vector3> point = new List<Vector3>(mesh.vertices);
		List<Vector2> uv = mesh.uv!=null?new List<Vector2>(mesh.uv):new List<Vector2>();
		if(mesh.normals==null || mesh.normals.Length==0) {
			createNormal(mesh,externalTriangles);
		}
		List<Vector3> pointn = new List<Vector3>(mesh.normals);
		int[] coord = externalTriangles;

		int leng=point.Count<=pointn.Count?point.Count:pointn.Count;
		int leng2=coord.Length/3;

		
		int i;int j;int k;int pos;
		int c0;int c1;int c2;int cmin;int cmax;int cminm;int cmaxm;

		int nonSpanCount=(vl-1)/vm+1;//原始区段数
			
		List<int> coordtCount=new List<int>(new int[nonSpanCount+1]);//平面索引下标统计数组
		int coordtCountLeng=coordtCount.Count;//平面索引下标统计数组的长度
		List<int> verticeSpanCount=new List<int>(new int[1]);//跨区段点数统计
		verticeSpanCount[0]=0;
		int verticeSpanCountLeng=verticeSpanCount.Count;
		for(i=0;i<coordtCountLeng;i++){//平面统计初始化
			coordtCount[i]=0;
		}
		for(i=0;i<leng2;i++){//平面和顶点分组分类计数
			pos=i*3;
			c0=coord[pos];
			c1=coord[pos+1];
			c2=coord[pos+2];
			cmin=c0<=c1?c0:c1;
			cmin=cmin<=c2?cmin:c2;
			cmax=c0>=c1?c0:c1;
			cmax=cmax>=c2?cmax:c2;
				
			cminm=cmin/vm;
			cmaxm=cmax/vm;
				
			if(cminm!=cmaxm){
				if(verticeSpanCount[verticeSpanCountLeng-1]+3>=vm){
					coordtCount.Add(0);
					coordtCountLeng++;
					verticeSpanCount.Add(0);
					verticeSpanCountLeng++;
				}
				coordtCount[coordtCountLeng-1]+=3;
				verticeSpanCount[verticeSpanCountLeng-1]+=3;
			}else{
				coordtCount[cmaxm]+=3;
			}
		}

		
		bool willAttach=(verticeSpanCountLeng==1 && (vl+verticeSpanCount[0]-1)/vm+1==nonSpanCount);
		int vel=willAttach?nonSpanCount:coordtCountLeng;

		List<List<Vector3>> ve=new List<List<Vector3>>();
		List<List<Vector3>> ne=new List<List<Vector3>>();
		List<List<Vector2>> uve=new List<List<Vector2>>();
		
		List<List<int>> ie=new List<List<int>>();
		List<int> index=new List<int>();
			

		int vec=0;
		for(i=0;i<vel-1;i++){
			ve.Add(new List<Vector3>());
			ne.Add(new List<Vector3>());
			uve.Add(new List<Vector2>());
			ie.Add(new List<int>());
		}
		for(i=0;i<nonSpanCount-1;i++){//记录非跨区段部分顶点
			vec=i<nonSpanCount-2?vm:vl%vm;
			
			for(j=0;j<vec;j++){
				k=(i+1)*vm+j;
				ve[i].Add(new Vector3(point[k].x,point[k].y,point[k].z));
				ne[i].Add(new Vector3(pointn[k].x,pointn[k].y,pointn[k].z));
				if(uv.Count>0){
					uve[i].Add(new Vector3(uv[k].x,uv[k].y));
				}
			}
		}
		j=0;
		int coordmin=vm;
		int v0=willAttach?nonSpanCount-2:nonSpanCount-1;
		int verticeSpanCountPos=0;
		for(i=0;i<leng2;i++){//记录平面和跨区段部分顶点
			pos=i*3;
			c0=coord[pos];
			c1=coord[pos+1];
			c2=coord[pos+2];
			cmin=c0<=c1?c0:c1;
			cmin=cmin<=c2?cmin:c2;
			cmax=c0>=c1?c0:c1;
			cmax=cmax>=c2?cmax:c2;
				
			cminm=cmin/vm;
			cmaxm=cmax/vm;
									
			if(cminm!=cmaxm){
				ie[v0].Add(ve[v0].Count);
				ie[v0].Add(ve[v0].Count+1);
				ie[v0].Add(ve[v0].Count+2);
				

				ve[v0].Add(new Vector3(point[c0].x,point[c0].y,point[c0].z));
				ne[v0].Add(new Vector3(pointn[c0].x,pointn[c0].y,pointn[c0].z));
				if(uv.Count>0){
					uve[v0].Add(new Vector3(uv[c0].x,uv[c0].y));
				}
				
				ve[v0].Add(new Vector3(point[c1].x,point[c1].y,point[c1].z));
				ne[v0].Add(new Vector3(pointn[c1].x,pointn[c1].y,pointn[c1].z));
				if(uv.Count>0){
					uve[v0].Add(new Vector3(uv[c1].x,uv[c1].y));
				}

				ve[v0].Add(new Vector3(point[c2].x,point[c2].y,point[c2].z));
				ne[v0].Add(new Vector3(pointn[c2].x,pointn[c2].y,pointn[c2].z));
				if(uv.Count>0){
					uve[v0].Add(new Vector3(uv[c2].x,uv[c2].y));
				}

				if(!willAttach && verticeSpanCountPos<verticeSpanCountLeng && ve[v0].Count>=verticeSpanCount[verticeSpanCountPos]) {
					v0++;
					verticeSpanCountPos++;
				}
			}else if(cmaxm>0){
				coordmin=cmaxm*vm;
				ie[cmaxm-1].Add(coord[pos]-coordmin);
				ie[cmaxm-1].Add(coord[pos+1]-coordmin);
				ie[cmaxm-1].Add(coord[pos+2]-coordmin);
			} else {
				index.Add(coord[pos]);
				index.Add(coord[pos+1]);
				index.Add(coord[pos+2]);
			}
		}
		point.RemoveRange(vm, point.Count-(vm));
		pointn.RemoveRange(vm, pointn.Count-(vm));
		if(uv.Count>vm) {
			uv.RemoveRange(vm, uv.Count-(vm));
		}

		mesh.vertices=point.ToArray();
		mesh.triangles=index.ToArray();
		mesh.normals=pointn.ToArray();
		if(uv.Count>0) {
			mesh.uv=uv.ToArray();
		}

		for(i=0; i<vel-1; i++) {
			GameObject o=new GameObject(obj.name!=null?obj.name+"_sub"+(i+1):"_sub"+(i+1));
			o.transform.SetParent(obj.transform);
			o.transform.localPosition=new Vector3(0,0,0);
			o.transform.localEulerAngles=new Vector3(0,0,0);
			o.transform.localScale=new Vector3(1,1,1);
			MeshFilter mf = o.AddComponent<MeshFilter>();
			MeshRenderer mr = o.AddComponent<MeshRenderer>();
			mf.mesh.vertices=ve[i].ToArray();
			mf.mesh.triangles=ie[i].ToArray();
			mf.mesh.normals=ne[i].ToArray();
			if(uv.Count>0) {
				mf.mesh.uv=uve[i].ToArray();
			}
			MeshRenderer mr0=obj.GetComponent<MeshRenderer>();
			mr.sharedMaterial=mr0.material;
			mr.shadowCastingMode=mr0.shadowCastingMode;
			mr.receiveShadows=mr0.receiveShadows;
		}
	}

	/// <summary>
    /// 判断所有的uv值是否相同
    /// </summary>
	/// <param name="uv">uv数组</param>
	public static bool sameUV(Vector2[] uv){
		if(uv==null || uv.Length<=0){
			return true;
		}
		int i=0;
		int leng=uv.Length;
		Vector2 uvi0=new Vector2(uv[0][0],uv[0][1]);
		for(i=0;i<leng;i++){
			if(uv[i][0]!=uvi0[0] || uv[i][1]!=uvi0[1]){
				return false;
			}
		}
		return true;
	}
	
    /// <summary>
    /// 按球形自动生成形体的uv
    /// </summary>
	/// <param name="mesh">模型网格</param>
	public static void autoUV(Mesh mesh){
		if(mesh==null  || mesh.vertices.Length<=0){
			return;
		}
		
		int leng=mesh.vertices.Length;
		Vector3[] point=mesh.vertices;
		Vector2[] uv=new Vector2[leng];
		mesh.RecalculateBounds();
		float centx=(mesh.bounds.min.x+mesh.bounds.max.x)*0.5f;
		float centy=(mesh.bounds.min.y+mesh.bounds.max.y)*0.5f;
		float centz=(mesh.bounds.min.z+mesh.bounds.max.z)*0.5f;
		float dx=mesh.bounds.max.x-mesh.bounds.min.x;
		float dy=mesh.bounds.max.y-mesh.bounds.min.y;
		float dz=mesh.bounds.max.z-mesh.bounds.min.z;
		float r=Mathf.Max(0.001f,Mathf.Max(dx,dy,dz)*0.5f);
		float x0;
		float y0;
		float z0;
		
		for(int i=0;i<leng;i++){
			x0=point[i][0]-centx;
			y0=point[i][1]-centy;
			z0=point[i][2]-centz;

			if(x0!=0 || z0!=0){
				uv[i]=new Vector2(Mathf.Max(0.01f,Mathf.Min(0.99f,Mathf.Abs(Mathf.Atan2(z0,x0)/Mathf.PI))),0.5f+Mathf.Asin(y0/r)/Mathf.PI);
			}else{
				uv[i]=new Vector2(0.5f,0.5f+Mathf.Asin(y0/r)/Mathf.PI);
			}
		}
		mesh.uv=uv;
	}
}
