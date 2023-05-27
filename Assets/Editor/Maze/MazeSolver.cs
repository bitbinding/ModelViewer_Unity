
namespace mazeEditor {
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//迷宫求解器
//用于对迷宫求解
public class MazeSolver{
	//A星寻路参数
	private int _straightCost;
	private int _diagonalCost;
	private bool _autoAlgorithm;
	private int _stepLimit;
	private bool _reusePath;
	
	//内存优化，开/闭列表，路径列表

    /// <summary>
    /// 开列表结构为：F估价和，G已消耗，H预消耗，节点X，节点Y，父节点X，父节点Y
    /// </summary>
	private List<int> openList=new List<int>();

    /// <summary>
    /// 闭列表结构为：节点X，节点Y，父节点X，父节点Y
    /// </summary>
	private List<int> closeList=new List<int>();
	private List<int> pathList=new List<int>();
	
	//速度优化，节点访问情况列表，1在闭列表，0在开列表，-1不在任何表
	private List<int> nodeList=new List<int>();
	
	//构造函数，用于设置A星寻路参数
	//straightCost直线花费
	//diagonalCost斜线花费
	//autoAlgorithm自动选择算法，为假的话，会始终使用最快的估价，否则在允许走斜线时会选用略慢的算法以提升路径质量
	//stepLimit寻路步数限制，到此步数即停止寻路
	//reusePath是否重复使用路径数组，有助于节省内存，但无法产生副本
	public MazeSolver(int straightCost=10,int diagonalCost=14,bool autoAlgorithm=true,int stepLimit=0,bool reusePath=false){
		_straightCost=straightCost;
		_diagonalCost=diagonalCost;
		_autoAlgorithm=autoAlgorithm;
		_stepLimit=stepLimit;
		_reusePath=reusePath;
	}
	
	//简单寻路
	//sMazeMap源迷宫数组
	//sBlock源迷宫不可通行的表示法
	//startX起点X坐标（序号0自开始）
	//startY起点Y坐标（序号0自开始）
	//finishX终点X坐标（序号0自开始）
	//finishY终点Y坐标（序号0自开始）
	//diagAble是否可以走斜线
    /// <summary>
    /// 源迷宫的尺寸
    /// </summary>
	public List<int> pathfinding(List<List<bool>> sMazeMap,bool sBlock,int startX,int startY,int finishX,int finishY,bool diagAble=true){
		int sMazeWidth=sMazeMap.Count;
		int sMazeHeight=sMazeMap[0].Count;
		
		//排除不必要的寻路
		if(startX<0 || finishX<0 || startY<0 || finishY<0 || startX>=sMazeWidth || finishX>=sMazeWidth || startY>=sMazeHeight || finishY>=sMazeHeight || sMazeMap[finishX][finishY]==sBlock)
			return null;
		else if(startX==finishX && startY==finishY)
			return new List<int>{startX,startY};
		
		//要使用的估价函数
		bool useDiagonal=(_autoAlgorithm && diagAble);
		
		//清空列表
		openList.Clear();
		closeList.Clear();;
		nodeList.Clear();
		nodeList.Capacity=sMazeWidth*sMazeHeight;
		
		//将开始节点放入开列表
		openList.AddRange(new int[]{0,0,0,startX,startY,-1,-1});
		
		//填充nodeList
		for(int n=0; n<sMazeWidth*sMazeHeight; n++) {
			nodeList.Add(-1);
		}
		
		//A星寻路
		int i=0;
		int currentNodeG=0;
		int currentNodeX=0;
		int currentNodeY=0;
		int G=0;
		int H=0;
		while(true){
			//找出开列表F最小的节点
			currentNodeG=openList[1];
			currentNodeX=openList[3];
			currentNodeY=openList[4];
			
			//将该节点移入封闭列表
			closeList.Add(currentNodeX);
			closeList.Add(currentNodeY);
			closeList.Add(openList[5]);
			closeList.Add(openList[6]);
			nodeList[currentNodeY*sMazeWidth+currentNodeX]=1;
			popNode();

			bool willBreak=false;
			
			//检查当前节点的邻节点
			for(int dx=-1;dx<2;dx++){
				
				int adjNodeX=currentNodeX+dx;
				if(adjNodeX<0 || adjNodeX>=sMazeWidth)
					continue;
				
				for(int dy=-1;dy<2;dy++) {
					willBreak=false;
					if((diagAble || dx==0 || dy==0) && (dx!=0 || dy!=0)){
						int adjNodeY=currentNodeY+dy;
						if(adjNodeY<0 || adjNodeY>=sMazeHeight)
							continue;
						
						//检查是否抵达目的地
						if(adjNodeX==finishX && adjNodeY==finishY) {
							willBreak=true;
							break;
						}
						
						
						//排除障碍和闭节点
						if(sMazeMap[adjNodeX][adjNodeY]==sBlock || nodeList[adjNodeY*sMazeWidth+adjNodeX]==1)
							continue;
						
						//检查是否在开列表里
						G=currentNodeG+((dx==0 || dy==0)?_straightCost:_diagonalCost);
						if(nodeList[adjNodeY*sMazeWidth+adjNodeX]==-1){
							//添加新节点
							H=useDiagonal?diagonal(adjNodeX,adjNodeY,finishX,finishY):
								manhattan(adjNodeX,adjNodeY,finishX,finishY);
							pushNode(G+H,G,H,adjNodeX,adjNodeY,currentNodeX,currentNodeY);
							nodeList[adjNodeY*sMazeWidth+adjNodeX]=0;
						}else{
							for(i=openList.Count-4;i>=0;i-=7){
								
								if(openList[i]==adjNodeX && openList[i+1]==adjNodeY){
									break;
								}
							}
							if(G<openList[i-2])
							//更新已有节点
							raiseNode(i-3,openList[i-1]+G,G,openList[i-1],adjNodeX,adjNodeY,currentNodeX,currentNodeY);
						}
					}
				}

				if(willBreak) break;
			}
						
			//达到步数限制或者找不到路
			if((_stepLimit!=0 && openList.Count>_stepLimit) || openList.Count==0)
				return null;
			if(willBreak) break;
		}
		
		//输出路径
		List<int> path;
		if(_reusePath){
			path=pathList;
			path.Clear();
		}else{
			path=new List<int>();
		}
		path.Insert(0,finishY);
		path.Insert(0,finishX);
		path.Insert(0,currentNodeY);
		path.Insert(0,currentNodeX);
		i=closeList.Count-4;
		while(i!=0){
			path.Insert(0,closeList[i+3]);
			path.Insert(0,closeList[i+2]);
			for(i-=4;i>=0;i-=4){
				if(closeList[i]==path[0] && closeList[i+1]==path[1])
					break;
			}
		}
		return path;
	}
	
	//加权寻路
	//sMazeMap源迷宫数组
	//sBlock源迷宫不可通行的表示法
	//startX起点X坐标（序号0自开始）
	//startY起点Y坐标（序号0自开始）
	//finishX终点X坐标（序号0自开始）
	//finishY终点Y坐标（序号0自开始）
	//diagAble是否可以走斜线
    /// <summary>
    /// 源迷宫的尺寸
    /// </summary>
	public List<int> weightedPathfinding(List<List<bool>> sMazeMap,bool sBlock,int startX,int startY,int finishX,int finishY,bool diagAble=true){
		int sMazeWidth=sMazeMap.Count;
		int sMazeHeight=sMazeMap[0].Count;
		
		//排除不必要的寻路
		if(startX<0 || finishX<0 || startY<0 || finishY<0 || startX>=sMazeWidth || finishX>=sMazeWidth || startY>=sMazeHeight || finishY>=sMazeHeight || sMazeMap[finishX][finishY]==sBlock)
			return null;
		else if(startX==finishX && startY==finishY)
			return new List<int>{startX,startY};
		
		//要使用的估价函数
		bool useDiagonal=(_autoAlgorithm && diagAble);
		
		//清空列表
		openList.Clear();
		closeList.Clear();
		nodeList.Clear();
		nodeList.Capacity=sMazeWidth*sMazeHeight;
		
		//将开始节点放入开列表
		openList.AddRange(new int[]{0,0,0,startX,startY,-1,-1});
		
		//填充nodeList
		for(int n=0; n<sMazeWidth*sMazeHeight; n++) {
			nodeList.Add(-1);
		}
		
		//A星寻路
		int i=0;
		int currentNodeG=0;
		int currentNodeX=0;
		int currentNodeY=0;
		int G=0;
		int H=0;
		
		bool willBreak=false;
		while(true){
			//找出开列表F最小的节点
			currentNodeG=openList[1];
			currentNodeX=openList[3];
			currentNodeY=openList[4];
			
			//将该节点移入封闭列表
			closeList.Add(currentNodeX);
			closeList.Add(currentNodeY);
			closeList.Add(openList[5]);
			closeList.Add(openList[6]);
			nodeList[currentNodeY*sMazeWidth+currentNodeX]=1;
			popNode();
			
			//检查当前节点的邻节点
			for(int dx=-1;dx<2;dx++){
				
				int adjNodeX=currentNodeX+dx;
				if(adjNodeX<0 || adjNodeX>=sMazeWidth)
					continue;
				
				for(int dy=-1;dy<2;dy++) {
					willBreak=false;
					if((diagAble || dx==0 || dy==0) && (dx!=0 || dy!=0)){
						int adjNodeY=currentNodeY+dy;
						if(adjNodeY<0 || adjNodeY>=sMazeHeight)
							continue;
						
						//检查是否抵达目的地
						if(adjNodeX==finishX && adjNodeY==finishY){
							willBreak=true;
							break;
						}
						
						//排除障碍和闭节点
						if(sMazeMap[adjNodeX][adjNodeY]==sBlock || nodeList[adjNodeY*sMazeWidth+adjNodeX]==1)
							continue;
						
						//检查是否在开列表里
						G=currentNodeG+((dx==0 || dy==0)?_straightCost:_diagonalCost)+(sMazeMap[adjNodeX][adjNodeY]?1:0);
						if(nodeList[adjNodeY*sMazeWidth+adjNodeX]==-1){
							//添加新节点
							H=useDiagonal?diagonal(adjNodeX,adjNodeY,finishX,finishY):
								manhattan(adjNodeX,adjNodeY,finishX,finishY);
							pushNode(G+H,G,H,adjNodeX,adjNodeY,currentNodeX,currentNodeY);
							nodeList[adjNodeY*sMazeWidth+adjNodeX]=0;
						}else{
							for(i=openList.Count-4;i>=0;i-=7){
								
								if(openList[i]==adjNodeX && openList[i+1]==adjNodeY){
									break;
								}
							}
							if(G<openList[i-2])
							//更新已有节点
							raiseNode(i-3,openList[i-1]+G,G,openList[i-1],adjNodeX,adjNodeY,currentNodeX,currentNodeY);
						}
					}
				}
				if(willBreak) break;
			}
						
			//达到步数限制或者找不到路
			if((_stepLimit!=0 && openList.Count>_stepLimit) || openList.Count==0)
				return null;
			if(willBreak) break;
		}
		
		//输出路径
		List<int> path;
		if(_reusePath){
			path=pathList;
			path.Clear();
		}else{
			path=new List<int>();
		}
		path.Insert(0,finishY);
		path.Insert(0,finishX);
		path.Insert(0,currentNodeY);
		path.Insert(0,currentNodeX);
		i=closeList.Count-4;
		while(i!=0){
			path.Insert(0,closeList[i+3]);
			path.Insert(0,closeList[i+2]);
			for(i-=4;i>=0;i-=4){
				if(closeList[i]==path[0] && closeList[i+1]==path[1])
					break;
			}
		}
		return path;
	}
	
	//manhattan估价算法，以直线距离为基准估价
	//作为优化，用?:代替了Math.abs与Math.min
	private int manhattan(int tx,int ty,int fx,int fy){
		return (((tx<fx)?(fx-tx):(tx-fx))+((ty<fy)?(fy-ty):(ty-fy)))*_straightCost;
	}
	
	//diagonal估价算法，以对角线和直线距离为基准
	//作为优化，用?:代替了Math.abs与Math.min
	private int diagonal(int tx,int ty,int fx,int fy){
		int dx=((tx<fx)?(fx-tx):(tx-fx));
      	int dy=((ty<fy)?(fy-ty):(ty-fy));
		int diag=((dx<dy)?dx:dy);
		return diag*_diagonalCost+_straightCost*(dx+dy-2*diag);
	}
	
	//=======二叉堆（开表）的操作=======
	
	//添加节点
	private void pushNode(int F,int G,int H,int x,int y,int px,int py){
		//把这个元素和根节点比较并交换
		int tmp=0;
		int i=0;
		for(i=openList.Count;i>0;i=tmp){
			
			tmp=((i/7-1)>>1)*7;
			if(F<openList[tmp]){
				openList[i]=openList[tmp];
				openList[i+1]=openList[tmp+1];
				openList[i+2]=openList[tmp+2];
				openList[i+3]=openList[tmp+3];
				openList[i+4]=openList[tmp+4];
				openList[i+5]=openList[tmp+5];
				openList[i+6]=openList[tmp+6];
			}else{
				break;
			}
		}
		openList[i]=F;
		openList[i+1]=G;
		openList[i+2]=H;
		openList[i+3]=x;
		openList[i+4]=y;
		openList[i+5]=px;
		openList[i+6]=py;
	}
	
	//删除节点
	private void popNode(){
		//和左右子树中较小的比较并交换
		int tmp=7;
		int len=openList.Count-7;
		int i=0;
		for(i=0;tmp<len;){
			
			if(tmp+7<len && openList[tmp+7]<openList[tmp])
				tmp+=7;
			
			if(openList[tmp]<openList[len]){
				openList[i]=openList[tmp];
				openList[i+1]=openList[tmp+1];
				openList[i+2]=openList[tmp+2];
				openList[i+3]=openList[tmp+3];
				openList[i+4]=openList[tmp+4];
				openList[i+5]=openList[tmp+5];
				openList[i+6]=openList[tmp+6];
			}else{
				break;
			}
			i=tmp;
			tmp=((i/7)*2+1)*7;
		}
		openList[i]=openList[len];
		openList[i+1]=openList[len+1];
		openList[i+2]=openList[len+2];
		openList[i+3]=openList[len+3];
		openList[i+4]=openList[len+4];
		openList[i+5]=openList[len+5];
		openList[i+6]=openList[len+6];
		//openList.Count=len;
	}
	
	//修改节点
	private void raiseNode(int index,int F,int G,int H,int x,int y,int px,int py){
		//把这个元素和根节点比较并交换
		int tmp=0;
		int i=0;
		for(i=index;i>0;i=tmp){
			
			tmp=((i/7-1)>>1)*7;
			if(F<openList[tmp]){
				openList[i]=openList[tmp];
				openList[i+1]=openList[tmp+1];
				openList[i+2]=openList[tmp+2];
				openList[i+3]=openList[tmp+3];
				openList[i+4]=openList[tmp+4];
				openList[i+5]=openList[tmp+5];
				openList[i+6]=openList[tmp+6];
			}else{
				break;
			}
		}
		openList[i]=F;
		openList[i+1]=G;
		openList[i+2]=H;
		openList[i+3]=x;
		openList[i+4]=y;
		openList[i+5]=px;
		openList[i+6]=py;
	}
}
}