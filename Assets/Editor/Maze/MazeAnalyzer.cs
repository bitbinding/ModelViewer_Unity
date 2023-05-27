
namespace mazeEditor {
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//迷宫分析器
//用于对迷宫进行数据分析
public class MazeAnalyzer{
	//迷宫宽度
	private int _mazeWidth=0;
	//迷宫高度
	private int _mazeHeight=0;
	//迷宫面积
	private int _mazeArea=0;
	//迷宫通路面积
	private int _pathArea=0;
	//迷宫通路面积百分比
	private float _mazeDensity=0;
	//迷宫死路数（可选）
	private int _deadEnd=0;
	//迷宫丁字路口数（可选）
	private int _crossing3=0;
	//迷宫十字路口数（可选）
	private int _crossing4=0;
	//迷宫路口（丁字+十字）总数（可选）
	private int _crossing=0;
	//迷宫边（连接节点的线段）总数（可选）
	private float _edge=0;
	//迷宫节点（路口+死路）总数（可选）
	private int _node=0;
	//迷宫岛数量（可选）
	private int _island=0;
	//岛数组（可选）
	private List<List<int>> _islandArray=null;

	private const int undefinedValue=-1;
	
	//为了只读的getter们
	public int mazeWidth{
		
	    get{return _mazeWidth;}
	}
	public int mazeHeight{
		
	    get{return _mazeHeight;}
	}
	public int mazeArea{
		
	    get{return _mazeArea;}
	}
	public int pathArea{
		
	    get{return _pathArea;}
	}
	public float mazeDensity{
		
	    get{return _mazeDensity;}
	}
	public int deadEnd{
		
	    get{return _deadEnd;}
	}
	public int crossing3{
		
	    get{return _crossing3;}
	}
	public int crossing4{
		
	    get{return _crossing4;}
	}
	public int crossing{
		
	    get{return _crossing;}
	}
	public float edge{
		
	    get{return _edge;}
	}
	public int node{
		
	    get{return _node;}
	}
	public int island{
		
	    get{return _island;}
	}
	public List<List<int>> islandArray{
		
	    get{return _islandArray;}
	}
	
	//分析迷宫
	//sMazeMap源迷宫数组
	//sBlock源迷宫不可通行的表示法
	//sNoBlock源迷宫可通行的表示法
	//analyzeConnection是否分析连通情况
	//analyzeIsland是否分析岛（封闭区域），真则返回岛数组
    /// <summary>
    /// 迷宫基本数据
    /// </summary>
	public void analyzeMaze(List<List<bool>> sMazeMap,bool sBlock=true,bool sNoBlock=false,bool analyzeConnection=false,bool analyzeIsland=false){
		_mazeWidth=sMazeMap.Count;
		_mazeHeight=sMazeMap[0].Count;
		_mazeArea=_mazeWidth*_mazeHeight;
		
		//迷宫连接情况初始化
		if(analyzeConnection){
			_deadEnd=0;
			_crossing3=0;
			_crossing4=0;
			_crossing=0;
			_edge=0;
			_node=0;
		}else{
			_deadEnd=-1;
			_crossing3=-1;
			_crossing4=-1;
			_crossing=-1;
			_edge=-1;
			_node=-1;
		}
		
		//迷宫岛情况初始化
		if(analyzeIsland){
			_islandArray=new List<List<int>>(new List<int>[_mazeWidth]);
			_island=0;
		}else{
			_islandArray=null;
			_island=-1;
		}
		
		//迷宫分析
		_pathArea=0;
		for(int sx=0;sx<_mazeWidth;sx++){
			
			if(analyzeIsland)
				_islandArray[sx]=new List<int>(new int[_mazeHeight]);
			for(int sy=0;sy<_mazeHeight;sy++) {

				_islandArray[sx][sy]=undefinedValue;
				if(sMazeMap[sx][sy]==sNoBlock){
					//统计通路面积
					_pathArea++;
					if(analyzeConnection){
						//分析连通情况
						int connection=0;
						if(sy>0 && sMazeMap[sx][sy-1]==sNoBlock){
							connection=1;
						}
						if(sx<_mazeWidth-1 && sMazeMap[sx+1][sy]==sNoBlock){
							connection++;
						}
						if(sy<_mazeHeight-1 && sMazeMap[sx][sy+1]==sNoBlock){
							connection++;
						}
						if(sx>0 && sMazeMap[sx-1][sy]==sNoBlock){
							connection++;
						}
						if(connection==0 && analyzeIsland){
							_island++;
							_islandArray[sx][sy]=_island;
						}else if(connection==1){
							_deadEnd++;
						}else if(connection==3){
							_crossing3++;
							_edge+=3;
						}else if(connection==4){
							_crossing4++;
							_edge+=4;
						}
					}
				}else if(analyzeIsland){
					_islandArray[sx][sy]=0;
				}
			}
		}
		
		//迷宫通路面积百分比
		_mazeDensity=_pathArea/(float)_mazeArea;
		
		//迷宫连接情况
		if(analyzeConnection){
			_crossing=_crossing3+_crossing4;
			_edge=(_edge+_deadEnd)/2;
			_node=_crossing+_deadEnd;
		}
		
		//生成岛数组
		if(analyzeIsland){
			for(int sx=0;sx<_mazeWidth;sx++){
				for(int sy=0;sy<_mazeHeight;sy++){
					if(_islandArray[sx][sy]==undefinedValue){
						_island++;
						exploreIsland(sx,sy);
					}
				}
			}
		}
	}
	//递归用的探索函数
	private void exploreIsland(int x,int y){
		_islandArray[x][y]=_island;
		if(x>0 && _islandArray[x-1][y]==undefinedValue){
			exploreIsland(x-1,y);
		}
		if(y>0 && _islandArray[x][y-1]==undefinedValue){
			exploreIsland(x,y-1);
		}
		if(x<_mazeWidth-1 && _islandArray[x+1][y]==undefinedValue){
			exploreIsland(x+1,y);
		}
		if(y<_mazeHeight-1 && _islandArray[x][y+1]==undefinedValue){
			exploreIsland(x,y+1);
		}
	}
}
}