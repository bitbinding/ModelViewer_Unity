
namespace mazeEditor {
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//随机普里姆法迷宫生成器
//随机普里姆法生成的迷宫岔路较多，整体上较为自然而又复杂
public class RPMazeCreater{
	//随机数种子
	private const double MAXRATIO=1/((double)uint.MaxValue+1);
	private uint _seed;
	
	//迷宫参数
	private bool _haveBorder;
	private bool _block;
	private bool _noBlock;
	
	//存邻墙的列表
	private List<int> blockPos=new List<int>();
	
	//迷宫数组
	private List<List<bool>> _mazeMap;
	
	//构造函数，用于设置统一的迷宫参数
	//haveBorder外围是否带有一圈边框
	//block不可通行的表示法
	//noBlock可通行的表示法
	public RPMazeCreater(bool haveBorder=false,bool block=true,bool noBlock=false){
		_haveBorder=haveBorder;
		_block=block;
		_noBlock=noBlock;
	}
	
	//产生迷宫数组
	//mazeWidth迷宫的宽度
	//mazeHeight迷宫的高度
	//startX起点X，默认随机（不算边框和墙壁，从0起）
	//startY起点Y，默认随机（不算边框和墙壁，从0起）
	//seed迷宫种子，默认随机
    /// <summary>
    /// 设置随机种子，0就使用时间做种
    /// </summary>
	public List<List<bool>> createMaze(int mazeWidth,int mazeHeight,int startX=-1,int startY=-1,uint seed=0){
	    _seed=seed!=0?seed:(uint)UnityEngine.Random.Range(0,0x7fffffff)+(uint)UnityEngine.Random.Range(0,0x7fffffff)+(uint)UnityEngine.Random.Range(0,1);
		
		//将返回的迷宫数组
		_mazeMap=new List<List<bool>>();
		
		//迷宫尺寸合法化
		if(mazeWidth<1)
			mazeWidth=1;
		if(mazeHeight<1)
			mazeHeight=1;
		
		//迷宫起点合法化
		if(startX<0 || startX>=mazeWidth)
			startX=(int)(rand()*mazeWidth);
		if(startY<0 || startY>=mazeHeight)
			startY=(int)(rand()*mazeHeight);
		
		//减去边框占的格子
		if(!_haveBorder){
			mazeWidth--;
			mazeHeight--;
		}
		
		//将迷宫的长宽换算成带墙的尺寸
		mazeWidth*=2;
		mazeHeight*=2;
		
		//将迷宫的起点换算成带墙的位置
		startX*=2;
		startY*=2;
		if(_haveBorder){
			startX++;
			startY++;
		}
		
		//产生出空白迷宫
		for(int x=0;x<=mazeWidth;x++){
			
			_mazeMap.Add(new List<bool>());
			for(int y=0;y<=mazeHeight;y++){
				
				_mazeMap[x].Add(_block);
			}
		}
		
		//产生迷宫
		randomizedPrim(startX,startY,mazeWidth-1,mazeHeight-1);
		
		//返回生成的迷宫
		return _mazeMap;
	}
	
	//使用随机普里姆法产生迷宫数组
	//参数为迷宫（树）带墙的起点位置和长宽限制尺寸
	private void randomizedPrim(int startX,int startY,int widthLimit,int heightLimit){
		//随机墙的索引
		int blockIndex=0;
		
		//将起点作为目标格
		int targetX=startX;
		int targetY=startY;
		
		//标记起点
		_mazeMap[targetX][targetY]=_noBlock;
		
		//记录邻墙
		if(targetY>1){
			blockPos.Add(targetX);
			blockPos.Add(targetY-1);
		}
		if(targetX<widthLimit){
			blockPos.Add(targetX+1);
			blockPos.Add(targetY);
		}
		if(targetY<heightLimit){
			blockPos.Add(targetX);
			blockPos.Add(targetY+1);
		}
		if(targetX>1){
			blockPos.Add(targetX-1);
			blockPos.Add(targetY);
		}

		while(blockPos.Count!=0){
			//随机选一面墙
			blockIndex=(int)(rand()*blockPos.Count/2.0f)*2;
			//找出此墙对面的目标格
			if(blockPos[blockIndex+1]>0 && _mazeMap[blockPos[blockIndex]][blockPos[blockIndex+1]-1]==_noBlock){
				targetX=blockPos[blockIndex];
				targetY=blockPos[blockIndex+1]+1;
			}else if(blockPos[blockIndex]+1<_mazeMap.Count && _mazeMap[blockPos[blockIndex]+1][blockPos[blockIndex+1]]==_noBlock){
				targetX=blockPos[blockIndex]-1;
				targetY=blockPos[blockIndex+1];
			}else if(_mazeMap[0].Count>0 && blockPos[blockIndex+1]+1<_mazeMap[0].Count && _mazeMap[blockPos[blockIndex]][blockPos[blockIndex+1]+1]==_noBlock){
				targetX=blockPos[blockIndex];
				targetY=blockPos[blockIndex+1]-1;
			}else if(blockPos[blockIndex]>0 && blockPos[blockIndex]>0 && _mazeMap[blockPos[blockIndex]-1][blockPos[blockIndex+1]]==_noBlock){
				targetX=blockPos[blockIndex]+1;
				targetY=blockPos[blockIndex+1];
			}
			
			//如果目标格尚未连通
			if(_mazeMap[targetX][targetY]==_block){
				//连通目标格
				_mazeMap[blockPos[blockIndex]][blockPos[blockIndex+1]]=_noBlock;
				_mazeMap[targetX][targetY]=_noBlock;
				//添加目标格的邻格
				if(targetY>1 && _mazeMap[targetX][targetY-1]==_block){
					blockPos.Add(targetX);
					blockPos.Add(targetY-1);
				}
				if(targetX<widthLimit && _mazeMap[targetX+1][targetY]==_block){
					blockPos.Add(targetX+1);
					blockPos.Add(targetY);
				}
				if(targetY<heightLimit && _mazeMap[targetX][targetY+1]==_block){
					blockPos.Add(targetX);
					blockPos.Add(targetY+1);
				}
				if(targetX>1 && _mazeMap[targetX-1][targetY]==_block){
					blockPos.Add(targetX-1);
					blockPos.Add(targetY);
				}
			}
			
			//移除此墙
			blockPos.RemoveRange(blockIndex,2);
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