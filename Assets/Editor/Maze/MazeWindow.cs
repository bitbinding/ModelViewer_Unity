using System.Collections.Generic;
using System.Text;

namespace mazeEditor
{
	using UnityEngine;
	using UnityEditor;
	using UnityEngine.UI;
	using System.IO;

	public class MazeWindow : EditorWindow
	{
		[MenuItem("Tools/自动生成迷宫")]
		public static void Open() {
			GetWindow<MazeWindow>("自动生成迷宫");
		}

		private static List<string> cw=new List<string> {"　", "口"};
		static List<string> cw1=new List<string>{"　","┃","━","┗","┃","┃","┏","┣","━","┛","━","┻","┓","┫","┳","╋"};
		

		private static string[] mazeTypes=new string[] {"出入口", "递归分割", "深度优先", "随机普里姆"};
		private static string[] mazeDescs=new string[] {
			"有出入口的迷宫",
			"递归分割法生成的迷宫较为简单，直路多且不扭曲，还可以生成“小房间”", 
			"深度优先法生成的迷宫极度扭曲，有着一条明显的主路", 
			"随机普里姆法生成的迷宫岔路较多，整体上较为自然而又复杂"};
		private static string[] outputTypes=new string[]{"方格","数字"};

		private Text outPutText;
		private int _width=10;
		private int _height=10;
		private int _zoom=1;
		private float _emptyRate=0f;
		private int mazeType=1;
		private int outputType=0;

		private string outputStr0="";
		

		private void OnEnable() {
		}


		private void OnGUI() {
			GUILayout.BeginVertical();
			GUILayout.Space(10);
			//outPutText=(Text) EditorGUILayout.ObjectField(new GUIContent("导出文字"),outPutText, typeof(Text), true);

			_width=EditorGUILayout.IntField(new GUIContent("宽度"),_width);
			_height=EditorGUILayout.IntField(new GUIContent("高度"),_height);
			_zoom=EditorGUILayout.IntField(new GUIContent("缩放"),_zoom);

			mazeType=EditorGUILayout.Popup(new GUIContent("迷宫类型：",mazeDescs[mazeType]),
				mazeType,mazeTypes);

			if(mazeType==1) {
				_emptyRate=EditorGUILayout.Slider(new GUIContent("空白比例(%)：", mazeDescs[mazeType]), _emptyRate, 0f, 100f);
			}
			
			int outputType1=EditorGUILayout.Popup(new GUIContent("导出方式："),outputType,outputTypes);
			if(outputType != outputType1) {
				outputType=outputType1;
				LogOutputStr();
			}

			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button(new GUIContent("生成",mazeDescs[mazeType]))) {
				Create();
			}

			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}
		
		static void addBorder(List<List<bool>> m,bool isBlock) {
			for(int i=0; i<m.Count; i++) {
				m[i].Insert(0,isBlock);
				m[i].Add(isBlock);
			}
			m.Insert(0,new List<bool>());
			for(int j=0; j<m[1].Count; j++) {
				m[0].Add(isBlock);
			}
			m.Add(new List<bool>());
			for(int j=0; j<m[0].Count; j++) {
				m[m.Count-1].Add(isBlock);
			}
		}
		
		static void removeBorder(List<List<bool>> m) {
			if(m.Count==0) return;
			for(int i=0; i<m.Count; i++) {
				if(m[i].Count==0) return;
				m[i].RemoveAt(m[i].Count-1);
				if(m[i].Count>0)m[i].RemoveAt(0);
			}
			m.RemoveAt(m.Count-1);
			if(m.Count>0)m.RemoveAt(0);
		}
		
		static List<List<bool>> copyMaze(List<List<bool>> m) {
			List<List<bool>> m1=new List<List<bool>>();
			for(int i=0; i<m.Count; i++) {
				m1.Add(new List<bool>(m[i]));
			}

			return m1;
		}

		static void zoomMaze(List<List<bool>> m,int zoomx,int zoomy) {
			List<List<bool>> m0=copyMaze(m);
			m.Clear();
			for(int i=0; i<m0.Count; i++) {
				for(int i1=0; i1<zoomx; i1++) {
					m.Add(new List<bool>());
					for(int j=0; j<m0[i].Count; j++) {
						for(int j1=0; j1<zoomy; j1++) {
							m[m.Count-1].Add(m0[i][j]);
						}
					}
				}
			}
		}
		
		
	
		static string getw1(List<List<bool>> m,int x,int y){
			return cw1[(y>0&&m[x][y-1]?1:0)|(x+1<m.Count&&m[x+1][y]?2:0)|(m.Count>0&&y+1<m[0].Count&&m[x][y+1]?4:0)|(x>0&&m[x-1][y]?8:0)];
		}
	
		static string getw(List<List<bool>> m,int x,int y){
			return cw[m[x][y]?1:0];
		}

		

		public static string toBlockString(List<List<bool>> m) {
			StringBuilder s=new StringBuilder();
			if(m.Count==0) return "";
			for(int j=0; j<m[0].Count; j++) {
				for(int i=0; i<m.Count; i++) {
					s.Append(getw(m, i, j));
				}
				if(j+1<m[0].Count) {
					s.Append("\n");
				}
			}

			return s.ToString();
		}
	
		public static string toEdgeString(List<List<bool>> m) {
			StringBuilder s=new StringBuilder();
			if(m.Count==0) return "";
			for(int j=0; j<m[0].Count; j++) {
				for(int i=0; i<m.Count; i++) {
					s.Append(getw1(m, i, j));
				}
				if(j+1<m[0].Count) {
					s.Append("\n");
				}
			}
			return s.ToString();
		}

		private void Create() {
			if(_width<3 || _height<3) {
				EditorUtility.DisplayDialog("","宽高度不能小于3。", "确定");
				return;
			}
			if(_zoom<0) {
				EditorUtility.DisplayDialog("","缩放值过小。", "确定");
				return;
			}
			if(_zoom>10) {
				EditorUtility.DisplayDialog("","缩放值过大。", "确定");
				return;
			}

			
			List<List<bool>> m=null;
			if(mazeType==0) {
				m=(new MiGong(_width,_height)).m;
				removeBorder(m);
			}else if(mazeType==1) {
				
				float rate0=_emptyRate/100f;
				int minArea=Mathf.Max(0,(int)(_width*_height*rate0*rate0));
				m=(new RDMazeCreater()).createMaze(_width, _height,0,minArea);
				addBorder(m,true);
			}else if(mazeType==2) {
				m=(new DFSMazeCreater()).createMaze(_width, _height);
				addBorder(m,true);
			}else if(mazeType==3) {
				m=(new RPMazeCreater()).createMaze(_width, _height);
				addBorder(m,true);
			}

			if(m!=null && _zoom>1) {
				zoomMaze(m,_zoom,_zoom);
			}
			if(m==null) {
				Debug.LogError("生成迷宫失败");
				return;
			}
			
			
			outputStr0=toBlockString(m);
			LogOutputStr();


			/*if(outPutText==null) {
				Canvas[] canvasArr=GameObject.FindObjectsOfType<Canvas>();
				if(canvasArr.Length==0) {
					EditorUtility.DisplayDialog("","需要先指定Canvas画布或Text文本以生成迷宫。", "确定");
					return;
				}

				Text text=(new GameObject()).AddComponent<Text>();
				text.transform.SetParent(canvasArr[canvasArr.Length-1].transform,false);
				text.raycastTarget=false;
				outPutText=text;
				Repaint();
			}
			outPutText.supportRichText=true;
			outPutText.horizontalOverflow=HorizontalWrapMode.Overflow;
			outPutText.verticalOverflow=VerticalWrapMode.Overflow;*/
		}

		void LogOutputStr() {
			if(outputStr0==null || outputStr0=="") return;
			string outputStr=outputStr0;

			if(outputType==1) {
				outputStr=outputStr.Replace(cw[0], "0").Replace(cw[1], "1").Replace('\n',' ');
			}
			Debug.Log(outputStr);
		}
	}
}