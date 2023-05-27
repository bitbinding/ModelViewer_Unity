
namespace mazeEditor {
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//递归分割法迷宫生成器
//递归分割法生成的迷宫较为简单，直路多且不扭曲，还可以生成“小房间”，此算法十分高效
public class RDMazeCreater{
	//随机数种子
	private const double MAXRATIO=1/((double)uint.MaxValue+1);
	private uint _seed;
	
	//迷宫参数
	private bool _haveBorder;
	private bool _block;
	private bool _noBlock;
	private int _minArea;
	
	//迷宫数组
	private List<List<bool>> _mazeMap;
	
	//构造函数，用于设置统一的迷宫参数
	//haveBorder外围是否带有一圈边框
	//minArea最小分割面积
	//block不可通行的表示法
	//noBlock可通行的表示法
	public RDMazeCreater(bool haveBorder=false,int minArea=4,bool block=true,bool noBlock=false){
		_haveBorder=haveBorder;
		_block=block;
		_noBlock=noBlock;
		_minArea=minArea;
	}
	
	//产生迷宫数组
	//mazeWidth迷宫的宽度
	//mazeHeight迷宫的高度
	//seed迷宫种子，默认随机
	//minArea临时最小分割面积
    /// <summary>
    /// 设置随机种子，0就使用时间做种
    /// </summary>
	public List<List<bool>> createMaze(int mazeWidth,int mazeHeight,uint seed=0,int minArea=0){
	    _seed=seed!=0?seed:(uint)UnityEngine.Random.Range(0,0x7fffffff)+(uint)UnityEngine.Random.Range(0,0x7fffffff)+(uint)UnityEngine.Random.Range(0,1);
		
		//临时最小分割面积
		int tmpMinArea=_minArea;
		if(minArea!=0){
			_minArea=minArea;
		}
		
		//将返回的迷宫数组
		_mazeMap=new List<List<bool>>();
		
		//迷宫尺寸合法化
		if(mazeWidth<1)
			mazeWidth=1;
		if(mazeHeight<1)
			mazeHeight=1;
		
		//减去边框占的格子
		if(!_haveBorder){
			mazeWidth--;
			mazeHeight--;
		}
		
		//将迷宫的长宽换算成带墙的尺寸
		mazeWidth*=2;
		mazeHeight*=2;
		
		//产生出空白迷宫
		for(int x=0;x<=mazeWidth;x++){
			
			_mazeMap.Add(new List<bool>());
			for(int y=0;y<=mazeHeight;y++){
				
				if(_haveBorder && (x==0 || y==0 || x==mazeWidth || y==mazeHeight)){
					_mazeMap[x].Add(_block);
				}else{
					_mazeMap[x].Add(_noBlock);
				}
			}
		}
		
		//产生迷宫
		if(_haveBorder){
			recursiveDivision(1,mazeWidth-1,1,mazeHeight-1);
		}else{
			recursiveDivision(0,mazeWidth,0,mazeHeight);
		}
		
		//恢复最小分割面积
		_minArea=tmpMinArea;
		
		//返回生成的迷宫
		return _mazeMap;
	}
	
	//使用递归分割法产生迷宫数组
	//参数为要分割区域的范围
	private void recursiveDivision(int left,int right,int top,int bottom){
		//检查是否达到了最小分割面积
		if(_minArea>4){
			if((right-left+1)*(bottom-top+1)<_minArea){
				return;
			}
		}
		
		//假设分割点不存在
		int dx=-1;
		int dy=-1;
		
		//产生随机分割点
		if(right-left>=2){
			dx=left+1+(int)(rand()*((right-left)/2.0f))*2;
		}
		if(bottom-top>=2){
			dy=top+1+(int)(rand()*((bottom-top)/2.0f))*2;
		}

		
		//没有继续分割的必要
		if(dx==-1 && dy==-1){
			return;
		}
		
		//补上墙壁
		if(dx!=-1){
			for(int y=top;y<=bottom;y++){
				
				_mazeMap[dx][y]=_block;
			}
		}
		if(dy!=-1){
			for(int x=left;x<=right;x++){
				
				_mazeMap[x][dy]=_block;
			}
		}
		
		//为确保连通，随机打通墙壁且不产生环路，并递归分割子区域
		if(dx!=-1 && dy!=-1){
			int side=(int)(rand()*4);
			if(side!=0){
				_mazeMap[dx][top+(int)(rand()*((dy-1-top)/2.0f+1))*2]=_noBlock;
			}
			if(side!=1){
				_mazeMap[dx+1+(int)(rand()*((right-dx-1)/2.0f+1))*2][dy]=_noBlock;
			}
			if(side!=2){
				_mazeMap[dx][dy+1+(int)(rand()*((bottom-dy-1)/2.0f+1))*2]=_noBlock;
			}
			if(side!=3){
				_mazeMap[left+(int)(rand()*((dx-1-left)/2.0f+1))*2][dy]=_noBlock;
			}
			recursiveDivision(left,dx-1,top,dy-1);
			recursiveDivision(dx+1,right,top,dy-1);
			recursiveDivision(dx+1,right,dy+1,bottom);
			recursiveDivision(left,dx-1,dy+1,bottom);
		}else if(dx==-1){
			_mazeMap[left+(int)(rand()*((right-left)/2.0f+1))*2][dy]=_noBlock;
			recursiveDivision(left,right,top,dy-1);
			recursiveDivision(left,right,dy+1,bottom);
		}else if(dy==-1){
			_mazeMap[dx][top+(int)(rand()*((bottom-top)/2.0f+1))*2]=_noBlock;
			recursiveDivision(left,dx-1,top,bottom);
			recursiveDivision(dx+1,right,top,bottom);
		}
	}
	
	//产生随机数
	private double rand() {
		ulong seed=_seed;
		seed^=(seed<<21);
		seed^=(seed>>35);
		seed^=(seed<<4);
		seed=seed&uint.MaxValue;
		_seed=(uint) seed;
		return seed*MAXRATIO;
	}
}
}