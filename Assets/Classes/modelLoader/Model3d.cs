using System;
using System.Collections.Generic;
using UnityEngine;


public class Model3d{
	
	const int functionMaxLeng=1001;

	public static string shaderName="Custom/CullOffShader";
	//static string shaderName="Standard";

	/// <summary>
	/// f(x,y)的数值和返回地址表
	/// </summary>
	int[] num1;

    /// <summary>
    /// f(x,y)的运算符表（为正时表示用的是数值，为负时表示用的是链接地址，为零时表示返回或结束）
    /// </summary>
	int[] oper1;
	

    /// <summary>
    /// g(x,y)的数值和返回地址表
    /// </summary>
	int[] num2;

    /// <summary>
    /// g(x,y)的运算符表（为正时表示用的是数值，为负时表示用的是链接地址，为零时表示返回或结束）
    /// </summary>
	int[] oper2;

    /// <summary>
    /// h(x,y)的数值和返回地址表
    /// </summary>
	int[] num3;

    /// <summary>
    /// h(x,y)的运算符表（为正时表示用的是数值，为负时表示用的是链接地址，为零时表示返回或结束）
    /// </summary>
	int[] oper3;

    /// <summary>
    /// f(x,y)的常量表
    /// </summary>
	float[] con1;

    /// <summary>
    /// g(x,y)的常量表
    /// </summary>
	float[] con2;

    /// <summary>
    /// h(x,y)的常量表
    /// </summary>
	float[] con3;
	
	const float stackMaxLeng=50;

    /// <summary>
    /// 公式编译用的字符串索引“栈”（表左括号后第一个字符的索引）
    /// </summary>
	int[] stack0;

    /// <summary>
    /// 公式编译用的返回地址“栈”（表括号内运算后返回的（数值和返回地址表）的地址）
    /// </summary>
	int[] stack1;

	/// <summary>
	/// 公式执行时用的“栈”
	/// </summary>
	float[] stackf;

	/// <summary>
	/// 编译用的“栈”顶端索引
	/// </summary>
	int stackTop=-1;
	
	int[] numLengArray=new int[]{0,0,0};
	int[] conLengArray=new int[]{0,0,0};
	//var strkh:String;//加了括号的字符串
	string strFunc1="";//f(x)的公式字符串
	string strFunc2="";//g(x)的公式字符串
	string strFunc3="";//h(x)的公式字符串
	int xseg;int yseg;//x方向分段，y方向分段
	string funcName="";//函数全名
	float[] cnt;//常量
	int drawType=0;//函数呈现方式
	string strResult1="empty";//函数编译结果情况
	string strResult2="empty";
	string strResult3="empty";
	//float scaleShape = 1.0f;//缩放比例
	float scaleU=1.0f;//u贴图坐标缩放比例
	float scaleV =1.0f;//v贴图坐标缩放比例
	bool uclosed=false;//模型为参数方程时，沿贴图u方向闭合
	bool vclosed=false;//模型为参数方程时，沿贴图v方向闭合
	bool usame0=false;//模型为参数方程时，沿贴图u方向闭合
	bool usamet=false;//模型为参数方程时，沿贴图v方向闭合
	bool vsame0=false;//模型为参数方程时，沿贴图u方向闭合
	bool vsamet=false;//模型为参数方程时，沿贴图v方向闭合
	int NaNMode=2;//非数值的处理方式
	float modelHeight=0;
	float minz=0;
	float maxz=-1;

	float ordinaryFunctionRange=10.0f;
	float parameticFunctionRange=2*Mathf.PI;
	float ordinaryFunctionScale=1;
	float parameticFunctionScale=1;

	public GameObject model = null;

	public string functionName{ 
		get{ 
			return this.funcName;
		}
	}
	
    /// <summary>
    /// 构造方法
    /// </summary>
	/// <param name="funcName0">函数名称</param>
	/// <param name="xseg0">u方向分段数</param>
	/// <param name="yseg0">v方向分段数</param>
	/// <param name="ordinaryFunctionRange0">非参数方程的定义域范围（关于原点对称）</param>
	/// <param name="ordinaryFunctionScale0">非参数方程的缩放系数</param>
	/// <param name="parameticFunctionScale0">参数方程的缩放系数</param>
	/// <param name="scaleU0">uv贴图中，u方向的缩放系数</param>
	/// <param name="scaleV0">uv贴图中，v方向的缩放系数</param>
	public Model3d(string funcName0="",int xseg0=100,int yseg0=100,float ordinaryFunctionRange0 = 10.0f,float ordinaryFunctionScale0 = 1.0f,float parameticFunctionScale0 = 1.0f,float scaleU0=1,float scaleV0=1){
		float a=0;float b=1;float c=1;float d=0.61803398874989484820f;float e=Mathf.Exp(1);float f=0.5772156649015328f;
		this.funcName=funcName0;
		this.ordinaryFunctionScale = ordinaryFunctionScale0;
		this.parameticFunctionScale = parameticFunctionScale0;
		this.xseg=xseg0;
		this.yseg=yseg0;
		this.cnt=new float[]{ a, b, c, d, e, f };
		int lf=functionMaxLeng;
		this.num1=new int[lf];//f(x,y)的数值和返回地址表
		this.oper1=new int[lf];//f(x,y)的运算符表（为正时表示用的是数值，为负时表示用的是链接地址，为零时表示返回或结束）
		
		this.num2=new int[lf];//g(x,y)的数值和返回地址表
		this.oper2=new int[lf];//g(x,y)的运算符表（为正时表示用的是数值，为负时表示用的是链接地址，为零时表示返回或结束）
		this.num3=new int[lf];//h(x,y)的数值和返回地址表
		this.oper3=new int[lf];//h(x,y)的运算符表（为正时表示用的是数值，为负时表示用的是链接地址，为零时表示返回或结束）
		this.con1=new float[lf];//f(x,y)的常量表
		this.con2=new float[lf];//g(x,y)的常量表
		this.con3=new float[lf];//h(x,y)的常量表

		int ls=functionMaxLeng;
		this.stack0=new int[ls];//公式编译用的字符串索引“栈”（表左括号后第一个字符的索引）
		this.stack1=new int[ls];//公式编译用的返回地址“栈”（表括号内运算后返回的（数值和返回地址表）的地址）
		this.stackf=new float[ls];


		//this.scaleShape=scaleShape0;
		this.scaleU=scaleU0;
		this.scaleV=scaleV0;
		this.updateFunction(funcName);
	}
	/// <summary>
	/// 更新函数图像
	/// </summary>
	/// <param name="funcName0">函数表达式</param>
	public void updateFunction(string funcName0){
		List<string> func = new List<string>(funcName0.Split(new char[] { ',' }));
		drawType=func.Count<=3?func.Count:3;
		if(drawType>=1){
			strFunc1=func[0];
			strFunc1=readstring(strFunc1,1);
			strResult1=compileWithStack(strFunc1,num1,oper1,con1,0);
		}else{
			strFunc1="";
		}
		//trace("______________________________");
		//trace(num1);
		//trace(oper1);
		//trace(con1);
		if(drawType>=2){
			strFunc2=func[1];
			strFunc2=readstring(strFunc2,2);
			strResult2=compileWithStack(strFunc2,num2,oper2,con2,1);
		}else{
			strFunc2="";
		}
		if(drawType>=3){
			strFunc3=func[2];
			strFunc3=readstring(strFunc3,3);
			strResult3=compileWithStack(strFunc3,num3,oper3,con3,2);
		}else{
			strFunc3="";
		}
		uclosed=false;
		vclosed=false;
		usame0=false;
		usamet=false;
		vsame0=false;
		vsamet=false;
		if(func.Count>3){
			uclosed=func[3].IndexOf("%")==0;
			vclosed=func[3].LastIndexOf("%")>0;
			usame0=vclosed && func[3].IndexOf("*")==0;
			usamet=vclosed && func[3].LastIndexOf("*")>0;
			vsame0=uclosed && func[3].IndexOf("*")==1;
			vsamet=uclosed && func[3].LastIndexOf("*")>1;
		}
		if(strFunc1=="" || strFunc2=="" || strFunc3==""){
			
		}
		prePlotGrid(true);
		modelHeight=maxz-minz;
		//Debug.Log(calculateArray(1f,0.0f));
		//plot();
	}

	string getFunctionName() {
		return this.funcName;
	}

	string readstring(string strFunc0,int numLengid=1){//预编译字符串，包括加入必要的乘号
		string strFunc=strFunc0;
		
		while(strFunc.IndexOf("\r")>=0){
			strFunc=strFunc.Replace("\r","");
		}
		while(strFunc.IndexOf(' ')>=0){
			strFunc=strFunc.Replace(" ","");
		}
		int strLeng=strFunc.Length;
		int i=0;int j=0;string nstrFunc;
		//int grade=1;
		//bool numflag=false;
		nstrFunc="";
		for(i=0;i<strLeng;i++){
			if (i >= 1 && (strFunc[i-1]>=48 && strFunc[i-1]<=57 || strFunc[i-1]=='.' || strFunc[i-1]==')' || strFunc[i-1]>=65 && strFunc[i-1]<=70 || strFunc[i-1]=='x' || strFunc[i-1]=='y' || strFunc[i-1]=='P') && (strFunc[i]=='x' || strFunc[i]=='y' || strFunc[i]=='P' || strFunc[i]=='(' || strFunc[i]>=65 && strFunc[i]<=70 || strFunc[i]>=97 && strFunc[i]<=119)){
				nstrFunc+=strFunc.Substring(j,i-j)+'*';
				j=i;
			}
		}
		nstrFunc+=strFunc.Substring(j,i-j);
		strFunc=nstrFunc;
		nstrFunc="";
		strLeng=strFunc.Length;
		j=0;
		
		stackTop=-1;
		
		return strFunc;
	}
	
	string compileWithStack(string strFunc,int[] num,int[] oper,float[] con,int numConLengId){//借助堆栈编译字符串，函数字符串可省略乘号和右括号
		int beginIndex=0;int returni=-1;
		int strLeng = strFunc.Length;
		numLengArray[numConLengId]=0;
		conLengArray[numConLengId]=0;
		int numLeng=0;
		int stackTopPrev=0;
		if(strFunc==""){
			numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "empty";
		}
		
		int i=beginIndex;int j=beginIndex;
		int khlevel=0;
		bool numflag=false;
		char operstr;
		int operstrj=0;
		int operi=0;
		int numBegin=numLeng;
		int numLeng2=0;
		bool firstPlusFlag=false;
		int stackTop0=stackTop+1;
		int stackTopt=stackTop0;
		bool willAdjustPower=false;
		bool firstTimesFlag=false;
		int numLeng2m=0;
		//string resultStr="";
		bool isNum=false;//判断某位是否可作为为数值、自变量或常量
		string strji="";//截取后的数值字符串
		
		do{
			i = beginIndex;
			j=beginIndex;
			khlevel=0;
			numflag=false;
			operstrj=0;
			operi=0;
			numBegin=numLeng;
			numLeng2=0;
			firstPlusFlag=false;
			stackTop0=stackTop+1;
			stackTopt=stackTop0;
			willAdjustPower=false;
			firstTimesFlag=false;
			numLeng2m=0;
			//resultStr="";
			isNum=false;//判断某位是否可作为为数值、自变量或常量
			strji="";//截取后的数值字符串
			
			if(numLeng>=functionMaxLeng-1){
				numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "numLeng error";
			}
			for(i=beginIndex;i<strLeng;i++){
				isNum=(strFunc[i]>=48 && strFunc[i]<=57 || strFunc[i]=='.' || strFunc[i]=='x' || strFunc[i]=='y' || strFunc[i]=='P' || strFunc[i]>=65 && strFunc[i]<=70);
				if(khlevel==0 && isNum){
					if(numflag==false){
						numflag=true;
						j=i;
					}
				}
				if(numflag && (!isNum || khlevel!=0 || i==strLeng-1)){
					numflag=false;
					strji=strFunc.Substring(j,(isNum?i+1:i)-j);
					if(conLengArray[numConLengId]>=100-1){
						numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "conLeng error";
					}

					float floatResult = 0.0f;
					if(strji=="x"){
						num[numLeng]=-1;
					}else if(strji=="P"){
						con[conLengArray[numConLengId]]=Mathf.PI;
						num[numLeng]=conLengArray[numConLengId];
						conLengArray[numConLengId]++;
					}else if(float.TryParse(strji, out floatResult)){
						con[conLengArray[numConLengId]]=Convert.ToSingle(strji);
						num[numLeng]=conLengArray[numConLengId];
						conLengArray[numConLengId]++;
					}else{
						num[numLeng]=-1;
					}
					
					if(j==beginIndex){
						oper[numLeng]=1;
					}else{
						operstr=strFunc[j-1];
						switch(operstr){
							case '+':oper[numLeng]=1;
								break;
							case '-':oper[numLeng]=2;
								break;
							case '*':oper[numLeng]=3;
								break;
							case '/':oper[numLeng]=4;
								break;
							case '^':oper[numLeng]=5;
								break;
							case '%':oper[numLeng]=6;
								break;
								/*case ')' :
									break;*/
							default:numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "opeator error";
						}
					}
					if(strji=="y" && oper[numLeng]>0){
						oper[numLeng]+=8;
					}else if(strji=="A" && oper[numLeng]>0){
						oper[numLeng]+=16;
					}else if(strji=="B" && oper[numLeng]>0){
						oper[numLeng]+=24;
					}else if(strji=="C" && oper[numLeng]>0){
						oper[numLeng]+=32;
					}else if(strji=="D" && oper[numLeng]>0){
						oper[numLeng]+=40;
					}else if(strji=="E" && oper[numLeng]>0){
						oper[numLeng]+=48;
					}else if(strji=="F" && oper[numLeng]>0){
						oper[numLeng]+=56;
					}
					if(num[numLeng]<0 && strji!="x" && oper[numLeng]<8){
						//未识别的数值
						numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;
						return "number error";
					}
					j=i;
					numLeng++;
				}
				if(i==strLeng-1 && !isNum && strFunc[i]!=')'){
					//函数不完整
					numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;
					return "not complete error";
				}
				if(strFunc[i]=='('){
					if(khlevel==0){
						num[numLeng]=-1;
						if(stackTop>=stackMaxLeng-1){
							numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "stack error";
						}else{
							stackTop++;
							stack0[stackTop]=i+1;
							stack1[stackTop]=numLeng;
						}
						if(i>=6 && strFunc.Substring(i-6,6) == "arcsin"){
							oper[numLeng]=-56;
							operstrj=i-7;
						}else if(i>=6 && strFunc.Substring(i-6,6)=="arccos"){
							oper[numLeng]=-64;
							operstrj=i-7;
						}else if(i>=6 && strFunc.Substring(i-6,6)=="arctan"){
							oper[numLeng]=-72;
							operstrj=i-7;
						}else if(i>=3 && strFunc.Substring(i-3,3)=="sin"){
							oper[numLeng]=-8;
							operstrj=i-4;
						}else if(i>=3 && strFunc.Substring(i-3,3)=="cos"){
							oper[numLeng]=-16;
							operstrj=i-4;
						}else if(i>=3 && strFunc.Substring(i-3,3)=="tan"){
							oper[numLeng]=-24;
							operstrj=i-4;
						}else if(i>=2 && strFunc.Substring(i-2,2)=="ln"){
							oper[numLeng]=-32;
							operstrj=i-3;
						}else if(i>=3 && strFunc.Substring(i-3,3)=="lif"){
							oper[numLeng]=-40;
							operstrj=i-4;
						}else if(i>=4 && strFunc.Substring(i-4,4)=="sqrt"){
							oper[numLeng]=-48;
							operstrj=i-5;
						}else if(i>=3 && strFunc.Substring(i-3,3)=="rif"){
							oper[numLeng]=-80;
							operstrj=i-4;
						}else if(i>=3 && strFunc.Substring(i-3,3)=="bif"){
							oper[numLeng]=-88;
							operstrj=i-4;
						}else if(i>=3 && strFunc.Substring(i-3,3)=="abs"){
							oper[numLeng]=-96;
							operstrj=i-4;
						}else if(i>=3 && strFunc.Substring(i-3,3)=="fif"){
							oper[numLeng]=-104;
							operstrj=i-4;
						}else if(i>=5 && strFunc.Substring(i-5,5)=="floor"){
							oper[numLeng]=-112;
							operstrj=i-6;
						}else{
							oper[numLeng]=0;
							operstrj=i-1;
						}
						if(operstrj<beginIndex){
							oper[numLeng]+=-1;
						}else{
							operstr=strFunc[operstrj];
							switch(operstr){
								case'+':oper[numLeng]+=-1;
									break;
								case'-':oper[numLeng]+=-2;
									break;
								case'*':oper[numLeng]+=-3;
									break;
								case'/':oper[numLeng]+=-4;
									break;
								case'^':oper[numLeng]+=-5;
									break;
								case'%':oper[numLeng]+=-6;
									break;
								case'(':break;
								default:numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "minus operator error";
							}
						}
						numLeng++;
						khlevel++;
					}else{
						khlevel++;
					}
				}else if(strFunc[i]==')'){
					if(khlevel==0){
						num[numLeng]=returni;
						oper[numLeng]=0;
						numLeng++;
						break;
					}else if(khlevel>0){
						khlevel--;
					}else{
						numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;
						return "quote error";
					}
				}
				if(numLeng>=100-1){
					numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "numLeng error";
				}
			}
			
			if(numLeng>0 && oper[numLeng-1]!=0 || numLeng==0){
				
				num[numLeng]=returni;
				
				oper[numLeng]=0;
				numLeng++;
				
			}
			numLeng2=numLeng;
			if(numLeng>=100-1){
				numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "numLeng error";
			}
			numLeng2=numLeng;
			firstPlusFlag=true;
			firstTimesFlag=true;
			//trace(numLeng);
			for(i=numBegin;i<numLeng;i++){//抽取运算主干。抽取的是加减法，和第一项无取反运算时该项的乘除法，并将操作数改成抽取源的位置编号。
				operi=oper[i]>=0?(oper[i]&7):(oper[i]|120);
				if(operi==1 && i>numBegin || operi==2 || operi==-1 && i>numBegin || operi==-2){
					num[numLeng2]=i;
					//oper[numLeng2]=oper[i]>=0?oper[i]+(j<<7):oper[i]-(j<<7);
					oper[numLeng2]=oper[i];
					numLeng2++;
					firstPlusFlag=false;
				}else if(operi==1 || operi==-1 || (operi==3 || operi==4 || operi==5 || operi==6 || operi==-3 || operi==-4 || operi==-5 || operi==-6) && firstPlusFlag || operi==0){
					num[numLeng2]=i;
					oper[numLeng2]=oper[i];
					numLeng2++;
				}
				if(operi==1 || operi==-1 || operi==2 || operi==-2 || operi==0){
					firstTimesFlag=true;
				}else if(operi==3 || operi==4 || operi==6 || operi==-3 || operi==-4 || operi==-6){
					firstTimesFlag=false;
				}else if(!firstTimesFlag && (operi==5 || operi==-5)){
					willAdjustPower=true;
				}
				if(numLeng2>=100-1){
					numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "numLeng2 error";
				}
			}
			//cal1Count=((numLeng2-numLeng)>>1);
			numLeng2m=numLeng2;
			firstPlusFlag=true;
			for(i=numLeng;i<numLeng2m && i<100-2;i++){//i++
				//j=oper[i]>=0?oper[i]>>7:(-oper[i])>>7;
				//oper+=oper[i]>0?-(j<<7):j<<7;
				j=(int)num[i];
				operi=oper[i]>=0?(oper[i]&7):(oper[i]|120);
				if(firstPlusFlag && (operi==1 && i>numLeng || operi==2 || operi==-1 && i>numLeng || operi==-2)){
					firstPlusFlag=false;
				}
				operi=oper[j+1]>=0?(oper[j+1]&7):(oper[j+1]|120);
				if(!firstPlusFlag && (operi==3 || operi==4 || operi==5 || operi==6 || operi==-3 || operi==-4 || operi==-5 || operi==-6)){
					if(oper[i]>0){
						num[i]=numLeng2-numLeng+numBegin;
						num[numLeng2]=num[j];
						oper[numLeng2]=((oper[i]&120)|1);
						oper[i]=-(oper[i]&7);//
						numLeng2++;
					}else if(oper[i]<0){
						num[i]=numLeng2-numLeng+numBegin;
						num[numLeng2]=num[j];
						oper[numLeng2]=(oper[i]|7);
						oper[i]=(oper[i]|120);
						while(stackTopt<=stackTop){
							if(stack1[stackTopt]==j){
								stack1[stackTopt]=(int)num[i];
								break;
							}
							stackTopt++;
						}
						numLeng2++;
					}else{
						num[i]=num[j];
						break;
					}
					if(numLeng2>=functionMaxLeng-1){
						numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "numLeng2 error";
					}
					operi=oper[j+1]>=0?(oper[j+1]&7):(oper[j+1]|120);
					firstTimesFlag=true;
					for(j=j+1;operi==3 || operi==4 || operi==5 || operi==6 || operi==-3 || operi==-4 || operi==-5 || operi==-6;j++){
						num[numLeng2]=num[j];
						oper[numLeng2]=oper[j];
						while(operi<0 && stackTopt<=stackTop){
							if(stack1[stackTopt]==j){
								stack1[stackTopt]=numLeng2-numLeng+numBegin;
								break;
							}
							stackTopt++;
						}
						numLeng2++;
						if(numLeng2>=functionMaxLeng-1){
							numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "numLeng2 error";
						}
						operi=oper[j+1]>=0?(oper[j+1]&7):(oper[j+1]|120);
					}
					num[numLeng2]=i-numLeng+numBegin;
					oper[numLeng2]=0;
					numLeng2++;
					if(numLeng2>=functionMaxLeng-1){
						numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "numLeng2 error";
					}
				}else{
					num[i]=num[j];
				}
				/*if (oper[i+1]==0) {
						num[i+1]=num[num[i+1]];
					}*/
			}
			for(i=numLeng,j=numBegin;i<numLeng2;i++,j++){
				//if (oper[i]==7) {
				//i++;
				//}
				num[j]=num[i];
				oper[j]=oper[i];
			}
			numLeng=j;
			if(willAdjustPower){
				numLeng2=numLeng;
				stackTopt=stackTop0;
				firstTimesFlag=true;
				//trace(numLeng);
				for(i=numBegin;i<numLeng;i++){//抽取运算主干。
					operi=oper[i]>=0?(oper[i]&7):(oper[i]|120);
					if(operi==1 || operi==-1 || operi==2 || operi==-2){
						num[numLeng2]=i;
						oper[numLeng2]=oper[i];
						numLeng2++;
						firstTimesFlag=true;
					}else if(operi==3 || operi==4 || operi==6 || operi==-3 || operi==-4 || operi==-6){
						num[numLeng2]=i;
						oper[numLeng2]=oper[i];
						numLeng2++;
						firstTimesFlag=false;
					}else if(firstTimesFlag && (operi==5 || operi==-5) || operi==0){
						num[numLeng2]=i;
						oper[numLeng2]=oper[i];
						numLeng2++;
					}
					if(numLeng2>=functionMaxLeng-1){
						numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "numLeng2 error";
					}
				}
				
				numLeng2m=numLeng2;
				firstTimesFlag=true;
				for(i=numLeng;i<numLeng2m && i<functionMaxLeng-2;i++){//i++
					//j=oper[i]>=0?oper[i]>>7:(-oper[i])>>7;
					//oper+=oper[i]>0?-(j<<7):j<<7;
					j=(int)num[i];
					operi=oper[i]>=0?(oper[i]&7):(oper[i]|120);
					if(operi==1 || operi==-1 || operi==2 || operi==-2){
						firstTimesFlag=true;
					}else if(operi==3 || operi==4 || operi==6 || operi==-3 || operi==-4 || operi==-6){
						firstTimesFlag=false;
					}
					operi=oper[j+1]>=0?(oper[j+1]&7):(oper[j+1]|120);
					if(!firstTimesFlag && (operi==5 || operi==-5)){
						if(oper[i]>0){
							num[i]=numLeng2-numLeng+numBegin;
							num[numLeng2]=num[j];
							oper[numLeng2]=((oper[i]&120)|1);
							oper[i]=-(oper[i]&7);//
							numLeng2++;
						}else if(oper[i]<0){
							num[i]=numLeng2-numLeng+numBegin;
							num[numLeng2]=num[j];
							oper[numLeng2]=(oper[i]|7);
							oper[i]=(oper[i]|120);
							stackTopt=stackTop0;
							while(stackTopt<=stackTop){
								if(stack1[stackTopt]==j){
									stack1[stackTopt]=(int)num[i];
									break;
								}
								stackTopt++;
							}
							numLeng2++;
						}else{
							num[i]=num[j];
							break;
						}
						if(numLeng2>=functionMaxLeng-1){
							numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "numLeng2 error";
						}
						operi=oper[j+1]>=0?(oper[j+1]&7):(oper[j+1]|120);
						firstTimesFlag=true;
						for(j=j+1;operi==5 || operi==-5;j++){
							num[numLeng2]=num[j];
							oper[numLeng2]=oper[j];
							stackTopt=stackTop0;
							while(operi<0 && stackTopt<=stackTop){
								if(stack1[stackTopt]==j){
									stack1[stackTopt]=numLeng2-numLeng+numBegin;
									break;
								}
								stackTopt++;
							}
							numLeng2++;
							if(numLeng2>=functionMaxLeng-1){
								numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "numLeng2 error";
							}
							operi=oper[j+1]>=0?(oper[j+1]&7):(oper[j+1]|120);
						}
						num[numLeng2]=i-numLeng+numBegin;
						oper[numLeng2]=0;
						numLeng2++;
						if(numLeng2>=functionMaxLeng-1){
							numLeng=0;numLengArray[numConLengId]=numLeng;conLengArray[numConLengId]=0;return "numLeng2 error";
						}
					}else{
						num[i]=num[j];
					}
					/*if (oper[i+1]==0) {
							num[i+1]=num[num[i+1]];
						}*/
				}
				for(i=numLeng,j=numBegin;i<numLeng2;i++,j++){
					//if (oper[i]==7) {
					//i++;
					//}
					num[j]=num[i];
					oper[j]=oper[i];
				}
				numLeng=j;
			}
			stackTopPrev=stackTop;
			if(stackTop>=0){
				//trace(stackTop);
				num[stack1[stackTop]]=numLeng;
				stackTop--;
				beginIndex = stack0[stackTop + 1];
				returni=stack1[stackTop+1];
			}
		}while(stackTopPrev>=0);
		
		numLengArray[numConLengId]=numLeng;
		
		return "complete";
	}
	public float calculateArray(float xt, float yt, int pos=0)
	{
		if (pos == 0)
		{
			return calculateArray(xt, yt, num1, oper1, con1, pos);
		}
		else if (pos == 1)
		{
			return calculateArray(xt, yt, num2, oper2, con2, pos);
		}
		else if (pos == 2)
		{
			return calculateArray(xt, yt, num3, oper3, con3, pos);
		}
		return float.NaN;
	}

	float calculateArray(float xt,float yt,int[] num,int[] oper,float[] con,int numConLengId){//计算函数
		int i=0;
		stackTop=0;
		stackf[0]=0;
		float numi=0.0f;
		int operi=0;
		int operfi=0;
		int numLeng=numLengArray[numConLengId];
		int conLeng=conLengArray[numConLengId];
		if(numLeng<=0 || numLeng>functionMaxLeng){
			return float.NaN;
		}
		if(conLeng>functionMaxLeng){
			return float.NaN;
		}
		//trace(xt);
		//var opergot:Number=0;
		for(i=0;i<numLeng;i++){
			if(oper[i]>0){
				numi=num[i]<0?(oper[i]<8?xt:(oper[i]<16?yt:cnt[(oper[i]>>3)-2])):(num[i]<conLeng?con[(int)num[i]]:float.NaN);
			}else{
				numi=num[i];
			}
			operi=oper[i]>=0?(oper[i]&7):(oper[i]|120);
			//operfi=oper[i]>=0?0:(-oper[i])>>3;
			if(float.IsNaN(stackf[stackTop])){
				return float.NaN;
			}else if(operi==1){
				stackf[stackTop]+=numi;
			}else if(operi==2){
				stackf[stackTop]-=numi;
			}else if(operi==3){
				stackf[stackTop]*=numi;
			}else if(operi==4){
				stackf[stackTop]=numi!=0?stackf[stackTop]/(float)numi:float.NaN;
			}else if(operi==5){
				stackf[stackTop]=(stackf[stackTop]==0 && numi<=0)?float.NaN:Mathf.Pow(stackf[stackTop],numi);
			}else if(operi==6){
				if(numi==0){
					stackf[stackTop]=float.NaN;
				}else{
					stackf[stackTop]=stackf[stackTop]-Mathf.Floor(stackf[stackTop]/(float)Mathf.Abs(numi))*Mathf.Abs(numi);
				}
			}else if(operi==0){
				if(stackTop<=0){
					return stackf[0];
				}else{
					if(Convert.ToInt32(num[i])<0){
						return float.NaN;
					}
					i=Convert.ToInt32(num[i]);
					//trace(stackf[stackTop]);
					numi=stackf[stackTop];
					operi=oper[i]>=0?(oper[i]&7):(oper[i]|120);
					operfi=oper[i]>=0?0:(-oper[i])>>3;
					if(float.IsNaN(numi)){
						return float.NaN;
					}else if(operfi==0){
					}else if(operfi==1){
						numi=Mathf.Sin(numi);
					}else if(operfi==2){
						numi=Mathf.Cos(numi);
					}else if(operfi==3){
						numi=Mathf.Tan(numi);
						if(numi>9.9f || numi<-9.9f)numi=float.NaN;
					}else if(operfi==4){
						numi=numi!=0?1.0f/Mathf.Log10(Mathf.Exp(1))*Mathf.Log10(numi):float.NaN;
					}else if(operfi==5){
						numi=xt>=numi?1:float.NaN;
					}else if(operfi==6){
						numi=Mathf.Sqrt(numi);
					}else if(operfi==7){
						numi=Mathf.Asin(numi);
					}else if(operfi==8){
						numi=Mathf.Acos(numi);
					}else if(operfi==9){
						numi=Mathf.Atan(numi);
					}else if(operfi==10){
						numi=xt<=numi?1:float.NaN;
					}else if(operfi==11){
						numi=yt>=numi?1:float.NaN;
					}else if(operfi==12){
						numi=Mathf.Abs(numi);
					}else if(operfi==13){
						numi=yt<=numi?1:float.NaN;
					}else if(operfi==14){
						numi=numi>=0?Convert.ToInt32(numi):Convert.ToInt32(numi)-1;
					}
					if(operi==-1){
						stackf[stackTop-1]+=numi;
					}else if(operi==-2){
						stackf[stackTop-1]-=numi;
					}else if(operi==-3){
						stackf[stackTop-1]*=numi;
					}else if(operi==-4){
						stackf[stackTop-1]=numi!=0?stackf[stackTop-1]/numi:float.NaN;
					}else if(operi==-5){
						stackf[stackTop-1]=(stackf[stackTop-1]==0 && numi<=0)?float.NaN:Mathf.Pow(stackf[stackTop-1],numi);
					}else if(operi==-6){
						stackf[stackTop-1]%=numi;
					}else{
						return float.NaN;
					}
					stackTop--;
				}
			}else if(operi<0){
				i=Convert.ToInt32(num[i])-1;
				if(stackTop>=50-1 || i<0){
					return float.NaN;
				}else{
					stackTop++;
					stackf[stackTop]=0;
				}
			}else{
				return float.NaN;
			}
		}
		return stackf[0];
	}
	
	public void prePlotGrid(bool updateCoord0=true,bool willCreateN=true){
		if (model != null)
		{
			GameObject.Destroy(model);
		}

		model = new GameObject();
		MeshFilter mf = model.AddComponent<MeshFilter>();
		MeshRenderer mr = model.AddComponent<MeshRenderer>();
		if (strResult1!="complete" || drawType>=2 && strResult2!="complete" || drawType>=3 && strResult3!="complete"){
			return;
		}
		Vector3[] point;
		Vector2[] uv;
		List<int> coord;
		List<int> coord2;
		//从函数生成平面信息
		int i=0;//循环变量		
		int j=0;
		int k=0;
		int l=0;
		int m=0;
		int n=0;
		
		//var zxy:Number;
		int xseg1=xseg+1;//行向分段数加一
		int yseg1=yseg+1;//列后向分段数加一
		int xsegp=(drawType==3 && uclosed)?xseg:xseg1;//每行的顶点数
		int ysegp=(drawType==3 && vclosed)?yseg:yseg1;//每列的顶点数
		int pointTotal=xsegp*ysegp;//总顶点数
		int coordTotal=xseg*yseg;//最大面积数
		
		int uvTotal=xseg1*yseg1;//最大面积数
		float xseg0=xseg/2.0f;//每行分段数（四边形个数）的一半
		float yseg0=yseg/2.0f;//每列分段数（四边形个数）的一半
		//trace(xseg0);
		int zseg=Mathf.Max(xseg1,yseg1);//横纵向分段数的较大值
		int shapeCount=drawType==2?2:1;//模型数
		float dxseg=parameticFunctionRange/xseg;//计算参数方程中过程中，x和y的增加幅度
		float dyseg=parameticFunctionRange/yseg;
		bool updateCoord = true;
		//Vector3 arrp;//point[i0]对应的数组
		int[] arrc=new int[4];//coord[i0]对应的数组
		bool coordAvailable=true;//四边形平面是否可用		
		bool hasNaNPoint=false;//是否含有值为NaN的顶点
		int myNaNMode=(!uclosed && !vclosed)?NaNMode:0;//参数方程中实际使用的非数值处理方式
		int coord2Total=(drawType>=3 && (uclosed || vclosed))?coordTotal:0;//coord2应有的长度
		int pointcur=0;//处理闭合曲面时用的当前顶点号

		point=new Vector3[pointTotal*shapeCount];
		uv= new Vector2[uvTotal*shapeCount];
		int uvCount = uvTotal*shapeCount;
		for(k=0;k<uvCount;k++){
			uv[k].Set(scaleU>=0 ? m/(float)xseg*scaleU : 1-m/(float)xseg*scaleU, scaleV>=0 ? n/(float)yseg*scaleV : 1-n/(float)yseg*scaleV);
			n++;
			if(n>=yseg1){
				n=0;
				m++;
			}
			if((k+1)%uvTotal==0){
				n=0;
				m=0;
			}
		}
		//trace(drawType>=3 && (uclosed || vclosed));

		int coordCount=coordTotal*shapeCount*6;
		int coord2Count=coord2Total*shapeCount*6;
		coord =new List<int>(new int[coordCount]);
		coord2=new List<int>(new int[coord2Count]);
		
		//trace(coord.length);
		l=0;
		i=0;
		j=0;
		k=0;
		if(drawType>=1 && drawType<3){
			hasNaNPoint=false;
			for(k=0;k<pointTotal;k++){
				point[k].x=ordinaryFunctionScale*(i-xseg0)/zseg;
				point[k].z=ordinaryFunctionScale*(j-yseg0)/zseg;
				point[k].y=ordinaryFunctionScale*calculateArray((i-xseg0)/zseg*ordinaryFunctionRange, (j-yseg0)/zseg*ordinaryFunctionRange,num1,oper1,con1,0)/ordinaryFunctionRange;
				if(float.IsNaN(point[k].y)){
					if(NaNMode==0){
						point[k].y=0;
					}
					hasNaNPoint=true;
					updateCoord=true;
				}
				j++;
				if(j>=yseg1){
					j=0;
					i++;
				}
			}

			i=0;
			j=0;
			for(k=0;k<pointTotal;k++){
				if(updateCoord){
					if(i!=0 && j!=0 && l+5<coordCount){
						arrc[0]=k-1;
						arrc[1]=k;
						arrc[2]=k-yseg1-1;
						arrc[3]=k-yseg1;

						coord[l] = arrc[2];
						coord[l+1] = arrc[1];
						coord[l+2] = arrc[0];
						coord[l+3] = arrc[1];
						coord[l+4] = arrc[2];
						coord[l+5] = arrc[3];
						if (hasNaNPoint){
							coordAvailable=true;
							if (NaNMode>0 && (float.IsNaN(point[arrc[0]].y) || float.IsNaN(point[arrc[1]].y) || float.IsNaN(point[arrc[2]].y) || float.IsNaN(point[arrc[3]].y))){
								coordAvailable=false;
							}
							
							if(coordAvailable){
								l+=6;
							}
						}else{
							l+=6;
						}
					}
				}
				j++;
				if(j>=yseg1){
					j=0;
					i++;
				}
				if(l>=coordCount){
					break;
				}
			}

			if(NaNMode!=0 && hasNaNPoint){
				for(k=0;k<pointTotal;k++){
					if(float.IsNaN(point[k].y)){
						point[k].y=0;
					}
				}
			}
			//gridWid=ordinaryFunctionScale*xseg/(float)scaleU/(float)zseg;
			//gridHei=ordinaryFunctionScale*yseg/(float)scaleV/(float)zseg;
			
		}else if(drawType==3){
			for(k=0;k<pointTotal;k++){
				point[k].x=parameticFunctionScale*calculateArray(i*dxseg,j*dyseg,num1,oper1,con1,0);
				point[k].z=parameticFunctionScale*calculateArray(i*dxseg,j*dyseg,num2,oper2,con2,1);
				point[k].y=parameticFunctionScale*calculateArray(i*dxseg,j*dyseg,num3,oper3,con3,2);
				if(float.IsNaN(point[k].x)){
					if(myNaNMode==0){
						point[k].x = 0;
					}
					hasNaNPoint=true;
					updateCoord=true;
				}
				if(float.IsNaN(point[k].y)){
					if(myNaNMode==0){
						point[k].y=0;
					}
					hasNaNPoint=true;
					updateCoord=true;
				}
				if(float.IsNaN(point[k].z)){
					if(myNaNMode==0){
						point[k].z=0;
					}
					hasNaNPoint=true;
					updateCoord=true;
				}
				j++;
				if(j>=ysegp){
					j=0;
					i++;
				}
			}

			i=0;
			j=0;
			k=0;
			l=0;
			pointcur=0;
			for(k=0;k<uvTotal;k++){
				if(updateCoord){
					if(i!=0 && j!=0 && l+5<coordCount && (coord2Count<=0 || l+5<coord2Count)){
						arrc[0]=k-1;
						arrc[1]=k;
						arrc[2]=k-yseg1-1;
						arrc[3]=k-yseg1;


						if(coord2Count>0) {
							coord2[l] = arrc[2];
							coord2[l+1] = arrc[1];
							coord2[l+2] = arrc[0];
							coord2[l+3] = arrc[1];
							coord2[l+4] = arrc[2];
							coord2[l+5] = arrc[3];

							pointcur=i*ysegp+j;
							arrc[0]=(uclosed && i==xseg)?j-1:pointcur-1;
							arrc[1]=(vclosed && j==yseg)?((uclosed && i==xseg)?0:pointcur-j):((uclosed && i==xseg)?j:pointcur);
							arrc[2]=pointcur-ysegp-1;
							arrc[3]=(vclosed && j==yseg)?pointcur-ysegp-j:pointcur-ysegp;

							if(usame0 && i==1){
								arrc[2]=0;
								arrc[3]=0;
							}
							if(usamet && i==xseg){
								arrc[0]=uclosed?0:i*ysegp;
								arrc[1]=uclosed?0:i*ysegp;
							}
							if(vsame0 && j==1){
								arrc[0]=0;
								arrc[2]=0;
							}
							if(vsamet && j==yseg){
								arrc[1]=vclosed?0:ysegp-1;
								arrc[3]=vclosed?0:ysegp-1;
							}
						}
						
						coord[l] = arrc[2];
						coord[l+1] = arrc[1];
						coord[l+2] = arrc[0];
						coord[l+3] = arrc[1];
						coord[l+4] = arrc[2];
						coord[l+5] = arrc[3];
						if(hasNaNPoint){
							coordAvailable=true;

							if (myNaNMode>0 && (float.IsNaN(point[arrc[0]].x) || float.IsNaN(point[arrc[0]].y) || float.IsNaN(point[arrc[0]].z) || float.IsNaN(point[arrc[1]].x) || float.IsNaN(point[arrc[1]].y) || float.IsNaN(point[arrc[1]].z) || float.IsNaN(point[arrc[2]].x) || float.IsNaN(point[arrc[2]].y) || float.IsNaN(point[arrc[2]].z) || float.IsNaN(point[arrc[3]].x) || float.IsNaN(point[arrc[3]].y) || float.IsNaN(point[arrc[3]].z))){
								coordAvailable=false;
							}
							if(coordAvailable){
								l+=6;
							}
						}else{
							l+=6;
						}
					}
					if(l>=coordCount){
						break;
					}
				}
				
				
				j++;
				if(j>=yseg1){
					j=0;
					i++;
				}
			}
			if(myNaNMode!=0 && hasNaNPoint){
				for(k=0;k<pointTotal;k++){
					if(float.IsNaN(point[k].x)){
						point[k].x=0;
					}
					if (float.IsNaN(point[k].y))
					{
						point[k].y = 0;
					}
					if (float.IsNaN(point[k].z))
					{
						point[k].z = 0;
					}
				}
			}
		}

		i=0;
		j=0;
		if(drawType==2){
			hasNaNPoint=false;
			for(k=pointTotal;k<(pointTotal<<1);k++){
				point[k].x=ordinaryFunctionScale*(i-xseg0)/zseg;
				point[k].z=ordinaryFunctionScale*(j-yseg0)/zseg;
				point[k].y=ordinaryFunctionScale*calculateArray((i-xseg0)/zseg*ordinaryFunctionRange, (j-yseg0)/zseg*ordinaryFunctionRange,num2,oper2,con2,1)/ordinaryFunctionRange;
				Vector3 p=point[k];
				if(float.IsNaN(point[k][2])){
					if(NaNMode==0){
						point[k][2]=0;
					}
					hasNaNPoint=true;
					updateCoord=true;
				}
				j++;
				if(j>=yseg1){
					j=0;
					i++;
				}
			}
			i=0;
			j=0;
			for(k=pointTotal;k<(pointTotal<<1);k++){
				if(updateCoord){
					if(i!=0 && j!=0 && l+5<coordCount){
						arrc[0]=k-1;
						arrc[1]=k;
						arrc[2]=k-yseg1-1;
						arrc[3]=k-yseg1;

						coord[l] = arrc[2];
						coord[l+1] = arrc[1];
						coord[l+2] = arrc[0];
						coord[l+3] = arrc[1];
						coord[l+4] = arrc[2];
						coord[l+5] = arrc[3];
						if (hasNaNPoint){
							coordAvailable=true;
							if(NaNMode>0 && (float.IsNaN(point[arrc[0]].y) || float.IsNaN(point[arrc[1]].y) || float.IsNaN(point[arrc[2]].y) || float.IsNaN(point[arrc[3]].y))){
								coordAvailable=false;
							}
							
							if(coordAvailable){
								l+=6;
							}
						}else{
							l+=6;
						}
					}
				}
				j++;
				if(j>=yseg1){
					j=0;
					i++;
				}
				if(l>=coordCount){
					break;
				}
			}
			if(NaNMode!=0 && hasNaNPoint){
				for(k=pointTotal;k<(pointTotal<<1);k++){
					if(float.IsNaN(point[k].y)){
						point[k].y=0;
					}
				}
			}
		}
		if (l < coord.Count)
		{
			coord.RemoveRange(l, coord.Count - l);
		}
		if (coord2.Count > 0 && l < coord2.Count)
		{
			coord2.RemoveRange(l, coord2.Count - l);
		}

		mr.material = new Material(Shader.Find(shaderName));
		mr.material.color = new Color(0.5f, 0.5f, 0.5f);
		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

		int[] coordArray=coord.ToArray();
		int[] coord2Array=coord2.ToArray();

		mf.mesh.vertices = point;
		mf.mesh.RecalculateBounds();
		MeshUtil.createNormal(mf.mesh,coordArray);
		if(coord2Array.Length>0) {
			MeshUtil.splitVerticesFromUv(mf.mesh,uv,coord2Array,coordArray);
		}else{
			mf.mesh.uv = uv;
		}
		MeshUtil.splitMeshFromLimit(model,coordArray);
		
	}

	/// <summary>
	/// 生成球
	/// </summary>
	public static GameObject Sphere(float r,int jseg=50,int wseg=50,bool invertU=false){
		string str="";
		if(!invertU){
			str=r+"*sin(y/2)cos(x),"+r+"*sin(y/2)sin(x),-"+r+"*cos(y/2),%**";
		}else{
			str=r+"*sin(y/2)cos(-x),"+r+"*sin(y/2)sin(-x),-"+r+"*cos(y/2),%**";
		}
		return (new Model3d(str,jseg,wseg)).model;
	}
	
    /// <summary>
    /// 生成圆锥侧面
    /// </summary>
	public static GameObject Cone(float r,float h,int jseg=50,int wseg=50){
		
		string str=(r/(2*Mathf.PI))+"(2P-y)cos(x),"+(r/(2*Mathf.PI))+"(2P-y)sin(x),"+(h/(2*Mathf.PI))+"(y-P),%";
		return (new Model3d(str,jseg,wseg)).model;
	}
	
    /// <summary>
    /// 生成圆台侧面
    /// </summary>
	public static GameObject ConeEx(float rtop, float rbottom,float h,int jseg=50,int wseg=50){
		float k=(rtop-rbottom)/(2*Mathf.PI);
		float b=rbottom;
		string str="("+k+"*y+"+b+")*cos(x),"+"("+k+"*y+"+b+")*sin(x),"+(h/(2*Mathf.PI))+"*(y-P),%";
		return (new Model3d(str,jseg,wseg)).model;
	}
	/// <summary>
	/// 生成圆柱侧面
	/// </summary>
	public static GameObject Cylinder(float r,float h,int jseg=50,int wseg=50){
		
		string str=r+"cos(x),"+r+"sin(x),"+(h/(2*Mathf.PI))+"(y-P),%";
		return (new Model3d(str,jseg,wseg)).model;
	}
	
    /// <summary>
    /// 生成圆环
    /// </summary>
	public static GameObject Ring(float r1,float r2,int jseg=50,int wseg=50){
		string str="("+r1+"+"+r2+"cos(y))*cos(x),("+r1+"+"+r2+"cos(y))*sin(x),"+r2+"sin(y),%%";
		return (new Model3d(str,jseg,wseg)).model;
	}
	
    /// <summary>
    /// 生成四边形ABCD构成的平面
    /// </summary>
	 public static GameObject Plane(Vector3 a, Vector3 b, Vector3 c, Vector3 d,int xseg=1,int yseg=1,int hideBack=0,float scaleU=1,float scaleV=1){
		int xseg1=xseg+1;
		int yseg1=yseg+1;
		int pointTotal=xseg1*yseg1;
		int coordTotal=xseg*yseg*6;
		int i=0;
		int j=0;
		int k=0;
		int l=0;
		Vector3[] point= new Vector3[pointTotal];
		int[] coord = new int[coordTotal];
		Vector2[] uv=new Vector2[point.Length];

		for(k=0;k<pointTotal;k++){
			point[k][0]=a[0]*i*j/(float)coordTotal+b[0]*(xseg-i)*j/(float)coordTotal+c[0]*(xseg-i)*(yseg-j)/coordTotal+d[0]*i*(yseg-j)/coordTotal;
			point[k][1]=a[1]*i*j/(float)coordTotal+b[1]*(xseg-i)*j/(float)coordTotal+c[1]*(xseg-i)*(yseg-j)/coordTotal+d[1]*i*(yseg-j)/coordTotal;
			point[k][2]=a[2]*i*j/(float)coordTotal+b[2]*(xseg-i)*j/(float)coordTotal+c[2]*(xseg-i)*(yseg-j)/coordTotal+d[2]*i*(yseg-j)/coordTotal;
			uv[k].Set(scaleU>=0 ? i/(float)xseg*scaleU : 1-i/(float)xseg*scaleU, scaleV>=0 ? j/(float)yseg*scaleV : 1-j/(float)yseg*scaleV);

			if(i!=0 && j!=0 && l+5<coordTotal){
				coord[l]=k-yseg1-1;
				coord[l+1]=k;
				coord[l+2]=k-1;
				coord[l+3]=k;
				coord[l+4]=k-yseg1-1;
				coord[l+5]=k-yseg1;
				l+=6;
			}
			j++;
			if(j>=yseg1){
				j=0;
				i++;
			}
		}

		GameObject model = new GameObject();
		MeshFilter mf = model.AddComponent<MeshFilter>();
		MeshRenderer mr = model.AddComponent<MeshRenderer>();

		mf.mesh.vertices = point;
		mf.mesh.triangles = coord;
		mf.mesh.uv = uv;
		//MeshUtil.createNormal(mf.mesh);
		mf.mesh.RecalculateNormals();
		mf.mesh.RecalculateBounds();

		mr.material = new Material(Shader.Find(shaderName));
		mr.material.color = new Color(0.5f, 0.5f, 0.5f);
		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

		return model;
	}
	
    /// <summary>
    /// 生成圆盘，圆盘的分段必须为偶数段
    /// </summary>
	public static GameObject RoundPlane(float r,int seg=50){
		int i;
		int j;
		int k;
		int l;
		int lprev;
		List<Vector3> point=new List<Vector3>();
		List<int[]> coord0=new List<int[]>();
		List<Vector2> uv=new List<Vector2>();
		if(seg%2!=0){
			seg++;
		}
		float dseg=2*Mathf.PI/seg;
		k=0;
		l=0;
		for(i=0;i<=(seg>>1);i++){
			lprev=l;
			l=i<=(seg>>2)?2*i+1:2*((seg>>1)-i)+1;//每行点数
			//trace(i,l,-(l>>1));
			for(j=0;j<l;j++){
				point.Add(new Vector3(r*Mathf.Sin((j-(l>>1))*dseg),0,r*Mathf.Cos(i*dseg)));
				uv.Add(new Vector2(0.5f*Mathf.Sin((j-(l>>1))*dseg)+0.5f,-0.5f*Mathf.Cos(i*dseg)+0.5f));
				
				if(i!=0){
					if(j==1 && i<=(seg>>2)){
						coord0.Add(new int[]{k-lprev-1,k-1,k,-1});
					}else if(j==0 && i>(seg>>1)-(seg>>2)){
						coord0.Add(new int[]{k-lprev,k,k-lprev+1,-1});
						if(j==l-1){
							coord0.Add(new int[]{k-l-1,k,k-l,-1});
						}
					}else if(j==l-1 && i<=(seg>>2)){
						coord0.Add(new int[]{k-l,k-1,k,-1});
					}else if(j==l-1 && i>(seg>>1)-(seg>>2)){
						coord0.Add(new int[]{k-lprev,k-1,k-lprev+1,k});
						coord0.Add(new int[]{k-l-1,k,k-l,-1});
					}else if(j>1 && i<=(seg>>2)){
						coord0.Add(new int[]{k-l,k-1,k-l+1,k});
					}else if(i>(seg>>1)-(seg>>2)){
						coord0.Add(new int[]{k-lprev,k-1,k-lprev+1,k});
					}else if(j>0){
						coord0.Add(new int[]{k-l-1,k-1,k-l,k});
					}
				}
				k++;
			}
		}
		List<int> coord = new List<int>();
		int count = coord0.Count;
		for(i=0;i< count; i++){
			int[] arrc = coord0[i];
			if (arrc.Length < 4){
				continue;
			}
			coord.Add(arrc[2]);
			coord.Add(arrc[1]);
			coord.Add(arrc[0]);
			if (arrc[3] >= 0) {
				coord.Add(arrc[1]);
				coord.Add(arrc[2]);
				coord.Add(arrc[3]);
			}
		}
		GameObject model = new GameObject();
		MeshFilter mf = model.AddComponent<MeshFilter>();
		MeshRenderer mr = model.AddComponent<MeshRenderer>();

		mf.mesh.vertices = point.ToArray();
		mf.mesh.triangles = coord.ToArray();
		mf.mesh.uv = uv.ToArray();
		//MeshUtil.createNormal(mf.mesh);
		mf.mesh.RecalculateNormals();
		mf.mesh.RecalculateBounds();

		mr.material = new Material(Shader.Find(shaderName));
		mr.material.color = new Color(0.5f, 0.5f, 0.5f);
		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

		return model;
	}
	
	public static GameObject Cuboid(float a0,float b0,float c0,bool verticalUV=false,bool invertU=false){
		float a=a0*0.5f;
		float b=b0*0.5f;
		float c=c0*0.5f;
		Vector3[] point0=new Vector3[]{new Vector3(-a,-b,-c),new Vector3(a,-b,-c),new Vector3(a,-b,c),new Vector3(-a,-b,c),new Vector3(-a,b,-c),new Vector3(a,b,-c),new Vector3(a,b,c),new Vector3(-a,b,c)};

		int[][] coord0=new int[][]{new int[]{2,3,6,7},new int[]{1,2,5,6},new int[]{0,1,4,5},new int[]{3,0,7,4},new int[]{4,5,7,6},new int[]{3,2,0,1}};

		Vector3[] point=new Vector3[24];
		int[] coord=new int[36];
		Vector2[] uv=new Vector2[24];

		for(int i=0;i<6;i++){
			if(!invertU) {
				point[i<<2].Set(point0[coord0[i][0]].x,point0[coord0[i][0]].y,point0[coord0[i][0]].z);
				point[i<<2|1].Set(point0[coord0[i][1]].x,point0[coord0[i][1]].y,point0[coord0[i][1]].z);
				point[i<<2|2].Set(point0[coord0[i][2]].x,point0[coord0[i][2]].y,point0[coord0[i][2]].z);
				point[i<<2|3].Set(point0[coord0[i][3]].x,point0[coord0[i][3]].y,point0[coord0[i][3]].z);
			}else{
				point[i<<2].Set(point0[coord0[i][1]].x,point0[coord0[i][1]].y,point0[coord0[i][1]].z);
				point[i<<2|1].Set(point0[coord0[i][0]].x,point0[coord0[i][0]].y,point0[coord0[i][0]].z);
				point[i<<2|2].Set(point0[coord0[i][3]].x,point0[coord0[i][3]].y,point0[coord0[i][3]].z);
				point[i<<2|3].Set(point0[coord0[i][2]].x,point0[coord0[i][2]].y,point0[coord0[i][2]].z);
			}
			

			coord[i*6]=i<<2|2;
			coord[i*6+1]=i<<2|1;
			coord[i*6+2]=i<<2;
			coord[i*6+3]=i<<2|1;
			coord[i*6+4]=i<<2|2;
			coord[i*6+5]=i<<2|3;

			
			if(!verticalUV){
				uv[i<<2].Set(i/6.0f,1);
				uv[i<<2|1].Set((i+1)/6.0f,1);
				uv[i<<2|2].Set(i/6.0f,0);
				uv[i<<2|3].Set((i+1)/6.0f,0);
			}else{
				uv[i<<2].Set(0,(i+1)/6.0f);
				uv[i<<2|1].Set(1,(i+1)/6.0f);
				uv[i<<2|2].Set(0,i/6.0f);
				uv[i<<2|3].Set(1,i/6.0f);
			}
			
		}

		GameObject model = new GameObject();
		MeshFilter mf = model.AddComponent<MeshFilter>();
		MeshRenderer mr = model.AddComponent<MeshRenderer>();

		mf.mesh.vertices = point;
		mf.mesh.triangles = coord;
		mf.mesh.uv = uv;
		//MeshUtil.createNormal(mf.mesh);
		mf.mesh.RecalculateNormals();
		mf.mesh.RecalculateBounds();

		mr.material = new Material(Shader.Find(shaderName));
		mr.material.color = new Color(0.5f, 0.5f, 0.5f);
		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

		return model;
	}
}