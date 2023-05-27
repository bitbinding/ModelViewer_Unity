
namespace mazeEditor {
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//迷宫滤镜
//用于对原始迷宫进行特定修改
public class MazeFilter{
	//随机数种子
	private const double MAXRATIO=1/((double)uint.MaxValue+1);
	private uint _seed;
	
	//源迷宫参数
	private bool _sBlock;
	private bool _sNoBlock;
	
	//目标迷宫数组
	private List<List<bool>> _dMazeMap;
	
	//构造函数，用于设置源迷宫参数
	//sBlock源迷宫不可通行的表示法
	//sNoBlock源迷宫可通行的表示法
	public MazeFilter(bool sBlock=true,bool sNoBlock=false){
		_sBlock=sBlock;
		_sNoBlock=sNoBlock;
	}
	
	//替换源迷宫标记
	//sMazeMap源迷宫数组
	//dBlock目标迷宫不可通行的表示法
	//dNoBlock目标迷宫可通行的表示法
	//createCopy是否创造新副本（深度拷贝）
    /// <summary>
    /// 源迷宫的尺寸
    /// </summary>
	public List<List<bool>> replaceFlag(List<List<bool>> sMazeMap,bool dBlock=true,bool dNoBlock=false,bool createCopy=false){
		int sMazeWidth=sMazeMap.Count;
		int sMazeHeight=sMazeMap[0].Count;
		
		//有需要就创建目标迷宫数组
		if(createCopy)
			_dMazeMap=new List<List<bool>>();
		
		//对源迷宫进行修改
		for(int sx=0;sx<sMazeWidth;sx++){
			if(createCopy)
				_dMazeMap[sx]=new List<bool>();
			for(int sy=0;sy<sMazeHeight;sy++){
				if(createCopy){
					_dMazeMap[sx][sy]=(sMazeMap[sx][sy]==_sBlock?dBlock:dNoBlock);
				}else{
					sMazeMap[sx][sy]=(sMazeMap[sx][sy]==_sBlock?dBlock:dNoBlock);
				}
			}
		}
		
		//有需要就返回目标迷宫数组
		if(createCopy)
		return _dMazeMap;
		
		//返回修改后的源数组
		return sMazeMap;
	}
	
	//加权替换源迷宫标记，使用给定的数组元素，根据上下左右格子是否与当前格一样来替换迷宫标记
	//sMazeMap源迷宫数组
	//dBlock障碍数组（四周一致情况按上右下左顺序对应从低到高四位二进制数，该数组与此二进制数的十进制表示法相对应）
	//dNoBlock通路数组（四周一致情况按上右下左顺序对应从低到高四位二进制数，该数组与此二进制数的十进制表示法相对应）
    /// <summary>
    /// 合法性验证
    /// </summary>
	public List<List<bool>> replaceWeightedFlag(List<List<bool>> sMazeMap,List<bool> dBlock,List<bool> dNoBlock=null){
		if((dBlock==null && dNoBlock==null) || (dBlock!=null && dBlock.Count!=16) || (dNoBlock!=null && dNoBlock.Count!=16))
		return null;
		
		//源迷宫的尺寸
		int sMazeWidth=sMazeMap.Count;
		int sMazeHeight=sMazeMap[0].Count;
		
		//创建目标迷宫数组
		_dMazeMap=new List<List<bool>>(new List<bool>[sMazeWidth]);
		
		//对目标迷宫进行填充
		int index=0;
		for(int sx=0;sx<sMazeWidth;sx++){
			
			_dMazeMap[sx]=new List<bool>(new bool[sMazeHeight]);
			for(int sy=0;sy<sMazeHeight;sy++){
				
				if((dBlock==null && sMazeMap[sx][sy]==_sBlock) || (dNoBlock==null && sMazeMap[sx][sy]==_sNoBlock)){
					_dMazeMap[sx][sy]=sMazeMap[sx][sy];
				}else{
					//检查连接情况
					index=0;
					if(sy>0 && sMazeMap[sx][sy-1]==sMazeMap[sx][sy]){
						index=1;
					}
					if(sx<sMazeWidth-1 && sMazeMap[sx+1][sy]==sMazeMap[sx][sy]){
						index+=2;
					}
					if(sy<sMazeHeight-1 && sMazeMap[sx][sy+1]==sMazeMap[sx][sy]){
						index+=4;
					}
					if(sx>0 && sMazeMap[sx-1][sy]==sMazeMap[sx][sy]){
						index+=8;
					}
					//用对应元素赋值
					if(sMazeMap[sx][sy]==_sBlock){
						_dMazeMap[sx][sy]=dBlock[index];
					}else{
						_dMazeMap[sx][sy]=dNoBlock[index];
					}
				}
			}
		}
		
		//返回目标迷宫数组
		return _dMazeMap;
	}
	
	//批量替换源迷宫标记
	//sMazeMap源迷宫数组
	//sFlagArray源迷宫需要替换的标记
	//dFlagArray目标迷宫对应的新标记
	//createCopy是否创造新副本（深度拷贝）
    /// <summary>
    /// 合法性验证
    /// </summary>
	public List<List<bool>> replaceMultipleFlag(List<List<bool>> sMazeMap,List<bool> sFlagArray,List<bool> dFlagArray,bool createCopy=false){
		if(sFlagArray.Count!=dFlagArray.Count)
		return null;
		
		//源迷宫的尺寸
		int sMazeWidth=sMazeMap.Count;
		int sMazeHeight=sMazeMap[0].Count;
		
		//有需要就创建目标迷宫数组
		if(createCopy)
		_dMazeMap=new List<List<bool>>(new List<bool>[sMazeWidth]);
		
		//对源迷宫进行修改
		int i=0;
		for(int sx=0;sx<sMazeWidth;sx++){
			
			if(createCopy)
			_dMazeMap[sx]=new List<bool>(new bool[sMazeHeight]);
			for(int sy=0;sy<sMazeHeight;sy++){
				
				i=sFlagArray.IndexOf(sMazeMap[sx][sy]);
				if(i==-1){
					if(createCopy)
					_dMazeMap[sx][sy]=sMazeMap[sx][sy];
				}else{
					if(createCopy){
						_dMazeMap[sx][sy]=dFlagArray[i];
					}else{
						sMazeMap[sx][sy]=dFlagArray[i];
					}
				}
			}
		}
		
		//有需要就返回目标迷宫数组
		if(createCopy)
		return _dMazeMap;
		
		//返回修改后的源数组
		return sMazeMap;
	}
	
	//添加迷宫边框
	//sMazeMap源迷宫数组
	//createCopy是否创造新副本（深度拷贝）
    /// <summary>
    /// 源迷宫的尺寸
    /// </summary>
	public List<List<bool>> addBorder(List<List<bool>> sMazeMap,bool createCopy=false){
		int sMazeWidth=sMazeMap.Count;
		int sMazeHeight=sMazeMap[0].Count;
		
		//有需要就创建目标迷宫数组
		if(createCopy)
			_dMazeMap=new List<List<bool>>(new List<bool>[sMazeWidth]);
			
		//添加源迷宫的最外圈格
		for(int sx=0;sx<sMazeWidth;sx++){
			
			if(createCopy){
				_dMazeMap[sx]=new List<bool>(new bool[sMazeHeight]);
				for(int sy=0;sy<sMazeHeight;sy++){
					
					_dMazeMap[sx][sy]=sMazeMap[sx][sy];
				}
				_dMazeMap[sx].Insert(0,_sBlock);
				_dMazeMap[sx].Add(_sBlock);
			}else{
				sMazeMap[sx].Insert(0,_sBlock);
				sMazeMap[sx].Add(_sBlock);
			}
		}
		
		if(createCopy){
			_dMazeMap.Insert(0,(new List<bool>()));
			_dMazeMap.Add(new List<bool>());
		}else{
			sMazeMap.Insert(0,(new List<bool>()));
			sMazeMap.Add(new List<bool>());
		}
		
		int dx=sMazeWidth+1;
		int newHeight=sMazeHeight+2;
		for(int dy=0;dy<newHeight;dy++){
			
			if(createCopy){
				_dMazeMap[0][dy]=_sBlock;
				_dMazeMap[dx][dy]=_sBlock;
			}else{
				sMazeMap[0][dy]=_sBlock;
				sMazeMap[dx][dy]=_sBlock;
			}
		}
			
		//有需要就返回目标迷宫数组
		if(createCopy)
		return _dMazeMap;
		
		//返回修改后的源数组
		return sMazeMap;
	}
	
	//去除迷宫边框
	//sMazeMap源迷宫数组
	//createCopy是否创造新副本（深度拷贝）
    /// <summary>
    /// 有需要就创建新迷宫数组
    /// </summary>
	public List<List<bool>> removeBorder(List<List<bool>> sMazeMap,bool createCopy=false){
		if(createCopy){
			//源迷宫的尺寸
			int sMazeWidth=sMazeMap.Count-1;
			int sMazeHeight=sMazeMap[0].Count-1;
			
			//有需要就创建目标迷宫数组
			_dMazeMap=new List<List<bool>>(new List<bool>[sMazeWidth]);
			
			//移除源迷宫的最外圈格
			int dx=0;
			for(int sx=1;sx<sMazeWidth;sx++){
				
				_dMazeMap[sx]=new List<bool>(new bool[sMazeHeight]);
				for(int sy=1;sy<sMazeHeight;sy++){
					
					_dMazeMap[dx][sy]=sMazeMap[sx][sy];
				}
				dx++;
			}
			
			//有需要就返回目标迷宫数组
			return _dMazeMap;
		}
		
		//移除源迷宫的最外圈格
		sMazeMap.RemoveAt(sMazeMap.Count-1);
		sMazeMap.RemoveAt(0);
		int newWidth=sMazeMap.Count;
		for(int x=0;x<newWidth;x++){
			
			sMazeMap[x].RemoveAt(sMazeMap[x].Count-1);
			sMazeMap[x].RemoveAt(0);
		}
		
		//返回修改后的源数组
		return sMazeMap;
	}
	
	//随机迷宫环路
	//sMazeMap源迷宫数组
	//loopRatio环路的比率（0-1，越大环路越多）
	//mustLoop结果是否必须是回路（为真时只考虑两侧是通路的墙，否则就随机拆除边框以外的墙）
	//seed随机数种子，默认随机
	//createCopy是否创造新副本（深度拷贝）
    /// <summary>
    /// 设置随机种子，0就使用时间做种
    /// </summary>
	public List<List<bool>> randomizedLoop(List<List<bool>> sMazeMap,float loopRatio=0.1f,bool mustLoop=true,uint seed=0,bool createCopy=false){
	    _seed=seed!=0?seed:(uint)UnityEngine.Random.Range(0,0x7fffffff)+(uint)UnityEngine.Random.Range(0,0x7fffffff)+(uint)UnityEngine.Random.Range(0,1);
		
		//源迷宫的尺寸
		int sMazeWidth=sMazeMap.Count-1;
		int sMazeHeight=sMazeMap[0].Count-1;
		
		//有需要就创建目标迷宫数组
		if(createCopy)
		_dMazeMap=new List<List<bool>>(new List<bool>[sMazeWidth]);
		
		//产生出随机环路后的迷宫
		for(int sx=0;sx<=sMazeWidth;sx++){
			
			if(createCopy)
			_dMazeMap[sx]=new List<bool>(new bool[sMazeHeight]);
			for(int sy=0;sy<=sMazeHeight;sy++){
				
				if(sx>0 && sx<sMazeWidth && sy>0 && sy<sMazeHeight && sMazeMap[sx][sy]==_sBlock && (!mustLoop || (sMazeMap[sx-1][sy]==_sNoBlock && sMazeMap[sx+1][sy]==_sNoBlock) || (sMazeMap[sx][sy-1]==_sNoBlock && sMazeMap[sx][sy+1]==_sNoBlock)) && rand()<loopRatio){
					if(createCopy){
						_dMazeMap[sx][sy]=_sNoBlock;
					}else{
						sMazeMap[sx][sy]=_sNoBlock;
					}
				}else if(createCopy){
					_dMazeMap[sx][sy]=sMazeMap[sx][sy];
				}
			}
		}
		
		//有需要就返回目标迷宫数组
		if(createCopy)
		return _dMazeMap;
		
		//返回修改后的源数组
		return sMazeMap;
	}
	
	//随机迷宫边缘
	//sMazeMap源迷宫数组
	//pathRatio通路的比率（0-1，越大通路越多）
	//pathOnly是否只修改通路格
	//blurMode是否模糊边线，是的话会拆除墙壁格细分后的内侧，反之则只拆除外侧
	//seed随机数种子，默认随机
    /// <summary>
    /// 设置随机种子，0就使用时间做种
    /// </summary>
	public List<List<bool>> randomizedEdge(List<List<bool>> sMazeMap,float pathRatio=0.75f,bool pathOnly=false,bool blurMode=false,uint seed=0){
	    _seed=seed!=0?seed:(uint)UnityEngine.Random.Range(0,0x7fffffff)+(uint)UnityEngine.Random.Range(0,0x7fffffff)+(uint)UnityEngine.Random.Range(0,1);
		
		//源迷宫的尺寸
		int sMazeWidth=sMazeMap.Count;
		int sMazeHeight=sMazeMap[0].Count;
		
		//将返回的目标迷宫数组
		_dMazeMap=new List<List<bool>>(new List<bool>[sMazeWidth]);
		
		//产生出随机边缘后的迷宫
		int mazeX=0;
		for(int sx=0;sx<sMazeWidth;sx++){
			
			for(int dx=-1;dx<2;dx++){
				
				_dMazeMap.Add(new List<bool>(new bool[sMazeHeight]));
				mazeX=sx*3+1+dx;
				for(int sy=0;sy<sMazeHeight;sy++){
					
					for(int dy=-1;dy<2;dy++){
						
						if(pathOnly && sMazeMap[sx][sy]==_sBlock){
							_dMazeMap[mazeX].Add(_sBlock);
						}else{
							if(sMazeMap[sx][sy]==_sNoBlock && (dx==0 || dy==0) && sx+dx!=-1 && sy+dy!=-1 && sx+dx!=sMazeWidth && sy+dy!=sMazeHeight && sMazeMap[sx+dx][sy+dy]==_sNoBlock){
								_dMazeMap[mazeX].Add(_sNoBlock);
							}else if(sMazeMap[sx][sy]==_sBlock && ((dx==0 && dy==0) || (((blurMode && (dx+dy)%2==0) || (!blurMode && (dx==0 || dy==0))) && sx+dx!=-1 && sy+dy!=-1 && sx+dx!=sMazeWidth && sy+dy!=sMazeHeight && ((dy!=0 && sMazeMap[sx][sy+dy]==_sBlock) || (dx!=0 && sMazeMap[sx+dx][sy]==_sBlock))))){
								_dMazeMap[mazeX].Add(_sBlock);
							}else{
								_dMazeMap[mazeX].Add(rand()<pathRatio?_sNoBlock:_sBlock);
							}
						}
					}
				}
			}
		}
		
		//返回目标迷宫
		return _dMazeMap;
	}
	
	//均一随机填充，使用给定的数组元素，等概率随机填充迷宫的通路和障碍
	//sMazeMap源迷宫数组
	//pathArray通路数组（元素间概率相等）
	//blockArray障碍数组（元素间概率相等）
	//seed随机数种子，默认随机
    /// <summary>
    /// 元素数量
    /// </summary>
	public List<List<bool>> uniformFill(List<List<bool>> sMazeMap,List<bool> pathArray=null,List<bool> blockArray=null,uint seed=0){
		int pathNum=(pathArray!=null?pathArray.Count:0);
		int blockNum=(blockArray!=null?blockArray.Count:0);
		
		//合法性验证
		if(pathNum+blockNum==0)
			return null;
		
		//设置随机种子，0就使用时间做种
		_seed=seed!=0?seed:(uint)UnityEngine.Random.Range(0,0x7fffffff)+(uint)UnityEngine.Random.Range(0,0x7fffffff)+(uint)UnityEngine.Random.Range(0,1);
		
		//源迷宫的尺寸
		int sMazeWidth=sMazeMap.Count;
		int sMazeHeight=sMazeMap[0].Count;
		
		//创建目标迷宫数组
		_dMazeMap=new List<List<bool>>(new List<bool>[sMazeWidth]);
		
		//对目标迷宫进行填充
		for(int sx=0;sx<sMazeWidth;sx++){
			
			_dMazeMap[sx]=new List<bool>(new bool[sMazeHeight]);
			for(int sy=0;sy<sMazeHeight;sy++){
				
				if(pathNum!=0 && sMazeMap[sx][sy]==_sNoBlock){
					_dMazeMap[sx][sy]=pathArray[(int)(rand()*pathNum)];
				}else if(blockNum!=0 && sMazeMap[sx][sy]==_sBlock){
					_dMazeMap[sx][sy]=blockArray[(int)(rand()*blockNum)];
				}else{
					_dMazeMap[sx][sy]=sMazeMap[sx][sy];
				}
			}
		}
		
		//返回目标迷宫数组
		return _dMazeMap;
	}
	
	//加权随机填充，使用给定的数组元素，按指定概率随机填充迷宫的通路和障碍
	//sMazeMap源迷宫数组
	//pathArray通路数组（按周围通路数量分为0-5六个数组）
	//pathWeight通路权值（元素概率为其概率/概率和）
	//blockArray障碍数组（按周围通路数量分为0-5六个数组）
	//blockWeight障碍权值（元素概率为其概率/概率和）
	//seed随机数种子，默认随机
	//内存优化，各组的权重和
	private List<int> pathWeightMax=new List<int>(new int[6]);
	private List<int> blockWeightMax=new List<int>(new int[6]);
    /// <summary>
    /// 合法性验证
    /// </summary>
	public List<List<bool>> weightedFill(List<List<bool>> sMazeMap,List<List<bool>> pathArray=null,List<List<int>> pathWeight=null,List<List<bool>> blockArray=null,List<List<int>> blockWeight=null,uint seed=0){
		if(pathArray==null && blockArray==null)
			return null;
		
		int g=0;
		//提取各组的权重和
		try{
			int i=0;
			for(g=0;g<6;g++){
				if(pathArray!=null){
					if(pathArray[g].Count==pathWeight[g].Count){
						pathWeightMax[g]=0;
						for(i=pathArray[g].Count-1;i>=0;i--){
							pathWeightMax[g]+=pathWeight[g][i];
						}
					}else{
						return null;
					}
				}
				if(blockArray!=null){
					if(blockArray[g].Count==blockWeight[g].Count){
						blockWeightMax[g]=0;
						for(i=blockArray[g].Count-1;i>=0;i--){
							blockWeightMax[g]+=blockWeight[g][i];
						}
					}else{
						return null;
					}
				}
			}
		}catch(Exception e){
			Debug.LogError(e);
			return null;
		}
		
		//设置随机种子，0就使用时间做种
		_seed=seed!=0?seed:(uint)UnityEngine.Random.Range(0,0x7fffffff)+(uint)UnityEngine.Random.Range(0,0x7fffffff)+(uint)UnityEngine.Random.Range(0,1);
		
		//源迷宫的尺寸
		int sMazeWidth=sMazeMap.Count;
		int sMazeHeight=sMazeMap[0].Count;
		
		//创建目标迷宫数组
		_dMazeMap=new List<List<bool>>(new List<bool>[sMazeWidth]);
		
		//对目标迷宫进行填充
		double r=0;
		for(int sx=0;sx<sMazeWidth;sx++){
			
			_dMazeMap[sx]=new List<bool>(new bool[sMazeHeight]);
			for(int sy=0;sy<sMazeHeight;sy++){
				if((blockArray==null && sMazeMap[sx][sy]==_sBlock) || (pathArray==null && sMazeMap[sx][sy]==_sNoBlock)){
					_dMazeMap[sx][sy]=sMazeMap[sx][sy];
				}else{
					//检查连通数
					g=0;
					if(sx>0 && sMazeMap[sx-1][sy]==_sNoBlock){
						g++;
					}
					if(sx<sMazeWidth-1 && sMazeMap[sx+1][sy]==_sNoBlock){
						g++;
					}
					if(sy>0 && sMazeMap[sx][sy-1]==_sNoBlock){
						g++;
					}
					if(sy<sMazeHeight-1 && sMazeMap[sx][sy+1]==_sNoBlock){
						g++;
					}
					if(g==4){
						if(sx>0 && sy>0 && sMazeMap[sx-1][sy-1]==_sNoBlock){
							g=5;
						}else if(sx<sMazeHeight-1 && sy>0 && sMazeMap[sx+1][sy-1]==_sNoBlock){
							g=5;
						}if(sx>0 && sy<sMazeHeight-1 && sMazeMap[sx-1][sy+1]==_sNoBlock){
							g=5;
						}if(sx<sMazeHeight-1 && sy<sMazeHeight-1 && sMazeMap[sx+1][sy+1]==_sNoBlock){
							g=5;
						}
					}
					//选出元素
					int i=0;
					if(sMazeMap[sx][sy]==_sNoBlock){
						r=rand()*pathWeightMax[g];
						for(i=pathWeight[g].Count-1;i>=0;i--){
							if(r<pathWeight[g][i]){
								break;
							}else{
								r-=pathWeight[g][i];
							}
						}
						_dMazeMap[sx].Add(pathArray[g][i]);
					}else{
						r=rand()*blockWeightMax[g];
						for(i=blockWeight[g].Count-1;i>=0;i--){
							if(r<blockWeight[g][i]){
								break;
							}else{
								r-=blockWeight[g][i];
							}
						}
						_dMazeMap[sx][sy]=blockArray[g][i];
					}
				}
			}
		}
		
		//返回目标迷宫数组
		return _dMazeMap;
	}
	
	//偏倚随机填充，使用给定的数组元素，按指定概率和优先级随机填充迷宫的通路和障碍
	//sMazeMap源迷宫数组
	//pathArray通路数组（按周围通路数量分为0-5六个数组，靠前的元素优先）
	//pathWeight通路概率（元素概率为其概率*之前元素未选中概率）
	//blockArray障碍数组（按周围通路数量分为0-5六个数组，靠前的元素优先）
	//blockWeight障碍概率（元素概率为其概率*之前元素未选中概率）
	//seed随机数种子，默认随机
    /// <summary>
    /// 合法性验证
    /// </summary>
	public List<List<bool>> biasFill(List<List<bool>> sMazeMap,List<List<bool>> pathArray=null,List<List<int>> pathWeight=null,List<List<bool>> blockArray=null,List<List<int>> blockWeight=null,uint seed=0){
		if(pathArray==null && blockArray==null)
		return null;

		int g;
		try{
			for(g=0;g<6;g++){
				if(pathArray!=null){
					if(pathArray[g].Count!=pathWeight[g].Count){
						return null;
					}
				}
				if(blockArray!=null){
					if(blockArray[g].Count!=blockWeight[g].Count){
						return null;
					}
				}
			}
		}catch(Exception e){
			Debug.LogError(e);
			return null;
		}
		
		//设置随机种子，0就使用时间做种
		_seed=seed!=0?seed:(uint)UnityEngine.Random.Range(0,0x7fffffff)+(uint)UnityEngine.Random.Range(0,0x7fffffff)+(uint)UnityEngine.Random.Range(0,1);
		
		//源迷宫的尺寸
		int sMazeWidth=sMazeMap.Count;
		int sMazeHeight=sMazeMap[0].Count;
		
		//创建目标迷宫数组
		_dMazeMap=new List<List<bool>>(new List<bool>[sMazeWidth]);
		
		//对目标迷宫进行填充
		int i=0;
		int len=0;
		g=0;
		for(int sx=0;sx<sMazeWidth;sx++){
			
			_dMazeMap[sx]=new List<bool>(new bool[sMazeHeight]);
			for(int sy=0;sy<sMazeHeight;sy++){
				
				if((blockArray==null && sMazeMap[sx][sy]==_sBlock) || (pathArray==null && sMazeMap[sx][sy]==_sNoBlock)){
					_dMazeMap[sx][sy]=sMazeMap[sx][sy];
				}else{
					//检查连通数
					g=0;
					if(sx>0 && sMazeMap[sx-1][sy]==_sNoBlock){
						g++;
					}
					if(sx<sMazeWidth-1 && sMazeMap[sx+1][sy]==_sNoBlock){
						g++;
					}
					if(sy>0 && sMazeMap[sx][sy-1]==_sNoBlock){
						g++;
					}
					if(sy<sMazeHeight-1 && sMazeMap[sx][sy+1]==_sNoBlock){
						g++;
					}
					if(g==4){
						if(sx>0 && sy>0 && sMazeMap[sx-1][sy-1]==_sNoBlock){
							g=5;
						}else if(sx<sMazeHeight-1 && sy>0 && sMazeMap[sx+1][sy-1]==_sNoBlock){
							g=5;
						}if(sx>0 && sy<sMazeHeight-1 && sMazeMap[sx-1][sy+1]==_sNoBlock){
							g=5;
						}if(sx<sMazeHeight-1 && sy<sMazeHeight-1 && sMazeMap[sx+1][sy+1]==_sNoBlock){
							g=5;
						}
					}
					//选出元素
					if(sMazeMap[sx][sy]==_sNoBlock){
						len=pathWeight[g].Count;
						for(i=0;i<len;i++){
							if(rand()<pathWeight[g][i]){
								break;
							}
						}
						if(i==len){
							_dMazeMap[sx][sy]=sMazeMap[sx][sy];
						}else{
							_dMazeMap[sx][sy]=pathArray[g][i];
						}
					}else{
						len=blockWeight[g].Count;
						for(i=0;i<len;i++){
							if(rand()<blockWeight[g][i]){
								break;
							}
						}
						if(i==len){
							_dMazeMap[sx][sy]=sMazeMap[sx][sy];
						}else{
							_dMazeMap[sx][sy]=blockArray[g][i];
						}
					}
				}
			}
		}
		
		//返回目标迷宫数组
		return _dMazeMap;
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