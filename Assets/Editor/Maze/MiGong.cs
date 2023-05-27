
namespace mazeEditor {
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class MiGong{
	
	public List<List<bool>> m;
	List<List<int>> d;
    /// <summary>
    /// for(var i:int=0;i<m.length;i++){
    /// trace(m[i]);
    /// }
    /// </summary>
	public MiGong(int x,int y){
		Make_Maze(x,y);
	}
	public void Make_Maze(int x,int y){
		int z1;int z2;
		int i;
		m=new List<List<bool>>(new List<bool>[2*x+3]);
		for(z1=0,z2=2*y+2;z1<=2*x+2;z1++){
			m[z1]=new List<bool>(new bool[2*y+3]);
			for(i=0;i<2*y+3;i++){
				m[z1][i]=true;
			}
			m[z1][0]=false;
			m[z1][z2]=false;
		}
		for(z1=0,z2=2*x+2;z1<=2*y+2;z1++){
			m[0][z1]=false;
			m[z2][z1]=false;
		}
		m[1][2]=false;m[2*x+1][2*y]=false;
		d=new List<List<int>>{new List<int>{0,1},new List<int>{1,0},new List<int>{0,-1},new List<int>{-1,0}};
		sr(Convert.ToInt32(UnityEngine.Random.Range(0f, 1f)*x)+1,Convert.ToInt32(UnityEngine.Random.Range(0f, 1f)*y)+1);
	}
	public int sr0(int x,int y){
		int zx=x*2;int zy=y*2;int nx;int tn=Convert.ToInt32(UnityEngine.Random.Range(0f, 1f)*2)>0?1:3;int i;
		m[zx][zy]=false;
		for(i=0,nx=Convert.ToInt32(UnityEngine.Random.Range(0f, 1f)*4);i<4;i++){
			if(m[zx+2*d[nx][0]][zy+2*d[nx][1]]){
				m[zx+d[nx][0]][zy+d[nx][1]]=false;
				sr0(x+d[nx][0],y+d[nx][1]);
			}
			nx=(nx+tn)%4;
		}
		return 0;
	}
	public int sr(int x,int y){
		int xt=x;int yt=y;
		int tn=UnityEngine.Random.Range(0, 2)>0?1:3;
		int nx=UnityEngine.Random.Range(0, 4);
		int i=0;
		List<List<int>> stack=new List<List<int>>{new List<int>{xt,yt,i,tn,nx}};
		int zx;int zy;
		List<int> arr;
		while(stack.Count>0){
			arr=stack[stack.Count-1];
			xt=arr[0];
			yt=arr[1];
			i=arr[2];
			tn=arr[3];
			nx=arr[4];
			zx=xt*2;
			zy=yt*2;
			m[zx][zy]=false;
			//trace(stack+"|",i,nx);
			//trace(stack.length);
			if(zx+2*d[nx][0]>=0 && zy+2*d[nx][1]>=0 && zx+2*d[nx][0]<m.Count && zy+2*d[nx][1]<m[0].Count) {
				if(m[zx+2*d[nx][0]][zy+2*d[nx][1]]) {
					m[zx+d[nx][0]][zy+d[nx][1]]=false;
					stack.Add(new List<int> {xt+d[nx][0], yt+d[nx][1], 0, UnityEngine.Random.Range(0, 2)>0 ? 1 : 3, UnityEngine.Random.Range(0, 4)});
					if(arr[2]<3) {
						arr[2]+=1;
						arr[4]=(nx+tn)&3;
					}

					continue;
				}
			}
			
			
			if(stack.Count>5){
				//break;
			}
			if(arr[2]<3){
				arr[2]+=1;
				arr[4]=(nx+tn)&3;
			}else{
				//trace(stack.length);
				stack.RemoveAt(stack.Count-1);
				//trace(stack.length);
			}
		}
		return 0;
	}
	
	
}
}