using System;
using UnityEngine;
using UnityEngine.UI;

//Converted from HslRgb.as
public class HslRgb{
	public HslRgb(){
	}
	public static string toHexString(int n){
		string c=Convert.ToString(n,16);
		switch(c.Length){
			case 1:c="00000"+c;
			 break;
			case 2:c="0000"+c;
			 break;
			 case 3:c="000"+c;
			 break;
			 case 4:c="00"+c;
			 break;
			 case 5:c="0"+c;
			 break;
			}
		return c.ToUpper();
	}
	public static int rgbToHex(float r,float g,float b){
		return (int)(Mathf.Round(r)*256*256+Mathf.Round(g)*256+Mathf.Round(b));
	}
	public static Vector3 hexToRgb(int val){
		return new Vector3((val>>16)&0xFF,(val>>8)&0xFF,val&0xFF);
	}
		//H=0~360,S=0-1,L=0-1
	public static Vector3 hslToRgb(float H,float S,float L){
		float p1;float p2;
		Vector3 rgb=new Vector3(0,0,0);
		if(L<=0.5f){
			p2=L*(1+S);
		}else{
			p2=L+S-(L*S);
		}
		p1=2*L-p2;
		if(S==0){
			rgb[0]=L;
			rgb[1]=L;
			rgb[2]=L;
		}else{
			rgb[0]=toRgb(p1,p2,H+120);
			rgb[1]=toRgb(p1,p2,H);
			rgb[2]=toRgb(p1,p2,H-120);
		}
		rgb[0]*=255;
		rgb[1]*=255;
		rgb[2]*=255;
		return rgb;
	}
	public static float toRgb(float q1,float q2,float hue){
		if(hue>360){
			hue=hue-360;
		}
		if(hue<0){
			hue=hue+360;
		}
		if(hue<60){
			return (q1+(q2-q1)*hue/60);
		}else if(hue<180){
			return (q2);
		}else if(hue<240){
			return (q1+(q2-q1)*(240-hue)/60.0f);
		}else{
			return (q1);
		}
	}
	public static Vector3 rgbToHsl(float R,float G,float B){
		R/=255;
		G/=255;
		B/=255;
		float max;float min;float diff;float r_dist;float g_dist;float b_dist;
		Vector3 hsl=new Vector3(0,0,0);
		max=Mathf.Max(Mathf.Max(R,G),B);
		min=Mathf.Min(Mathf.Min(R,G),B);
		diff=max-min;
		hsl[2]=(max+min)/2.0f;
		if(diff==0){
			hsl[0]=0;
			hsl[1]=0;
		}else{
			if(hsl[2]<0.5f){
			hsl[1]=diff/(max+min);
		}else{
			hsl[1]=diff/(2-max-min);
		}
		r_dist=(max-R)/diff;
		g_dist=(max-G)/diff;
		b_dist=(max-B)/diff;
		if(R==max){
			hsl[0]=b_dist-g_dist;
		}else if(G==max){
			hsl[0]=2+r_dist-b_dist;
		}else if(B==max){
			hsl[0]=4+g_dist-r_dist;
			}
		hsl[0]*=60;
		if(hsl[0]<0){
			hsl[0]+=360;
			}
		if(hsl[0]>=360){
			hsl[0]-=360;
			}
		}
		return hsl;
	}

	public static Color rgbToColor(float R,float G,float B){
		return new Color(R/255,G/255,B/255);
	}

	public static Color hexToColor(int hex){
		Vector3 rgb=hexToRgb(hex);
		return rgbToColor(rgb[0],rgb[1],rgb[2]);
	}

	public static Color hslToColor(float H,float S,float L){
		Vector3 rgb=hslToRgb(H,S,L);
		return rgbToColor(rgb[0],rgb[1],rgb[2]);
	}

	public static Vector3 colorToRgb(Color c){
		return new Vector3(c.r*255,c.g*255,c.b*255);
	}

	public static Vector3 colorToHsl(Color c){
		return rgbToHsl(c.r*255,c.g*255,c.b*255);
	}

	public static int colorToHex(Color c){
		return rgbToHex(c.r*255,c.g*255,c.b*255);
	}
}