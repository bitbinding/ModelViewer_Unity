using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TextureLoader
{
    /// <summary>
    /// 获取二进制图片文件的宽高
    /// </summary>
    public static int[] getImageFileSize(byte[] data){
        if(data==null)return new int[]{0,0};
        if(data.Length>4 && data[0]==0xff && data[1]==0xd8 && data[2]==0xff && data[3]==0xe0){
            //jpg
            int pos=2;
            while(pos+3<data.Length){
                if(data[pos]==0xff && data[pos+1]==0xc0 && pos+8<data.Length){
                    return new int[]{data[pos+7]<<8|data[pos+8],data[pos+5]<<8|data[pos+6]};
                }
                pos+=(data[pos+2]<<8|data[pos+3])+2;
            }
        }
        if(data.Length>4 && data[0]==0x89 && data[1]==0x50 && data[2]==0x4e && data[3]==0x47){
            //png
            int pos=8;
            while(pos+8<data.Length){
                if(pos+16<data.Length && data[pos+4]==0x49 && data[pos+5]==0x48 && data[pos+6]==0x44 && data[pos+7]==0x52){
                    return new int[]{data[pos+8]<<24|data[pos+9]<<16|data[pos+10]<<8|data[pos+11],
                            data[pos+12]<<24|data[pos+13]<<16|data[pos+14]<<8|data[pos+15]};
                }
                pos+=(data[pos]<<24|data[pos+1]<<16|data[pos+2]<<8|data[pos+3])+12;
            }
        }
        if(data.Length>9 && data[0]==0x47 && data[1]==0x49 && data[2]==0x46){
            //gif
            return new int[]{data[7]<<8|data[6],data[9]<<8|data[8]};
        }
        if(data.Length>26 && data[0]==0x42 && data[1]==0x4d){
            //bmp
            return new int[]{data[21]<<24|data[20]<<16|data[19]<<8|data[18],
                    data[25]<<24|data[24]<<16|data[23]<<8|data[22]};
        }
        if(data.Length>12 && data[0]==0x52 && data[1]==0x41 && data[2]==0x57){
            //raw
            return new int[]{data[4]<<24|data[5]<<16|data[6]<<8|data[7],
                    data[8]<<24|data[9]<<16|data[10]<<8|data[11]};
        }
        if(data.Length>8 && data[0]==0x3 && data[1]==0x5){
            //dat
            return new int[]{data[5]<<8|data[4],data[7]<<8|data[6]};
        }
        return new int[]{0,0};
    }

    /// <summary>
    /// 以IO方式进行加载
    /// </summary>
    public static Texture2D load(string url)
    {
        // double startTime = (double)Time.time;
        //创建文件读取流
        FileStream fileStream=null;
        try {
            fileStream = new FileStream(url, FileMode.Open, FileAccess.Read);
        } catch(IOException ex) {
            Debug.Log(ex.Message);
            fileStream=null;
        }

        if(fileStream==null) {
            return null;
        }
        
        fileStream.Seek(0, SeekOrigin.Begin);
        //创建文件长度缓冲区
        byte[] bytes = new byte[fileStream.Length];
        //读取文件
        fileStream.Read(bytes, 0, (int)fileStream.Length);
        //释放文件读取流
        fileStream.Close();
        fileStream.Dispose();
        fileStream = null;

        //创建Texture
        int[] size=getImageFileSize(bytes);
        if(size[0]<=0 || size[1]<=0) {
            size[0]=2048;
            size[1]=2048;
        }
        Texture2D texture = new Texture2D(size[0],size[1]);
        texture.LoadImage(bytes);

        return texture;
    }

}
