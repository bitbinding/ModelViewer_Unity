using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

public static class MeshCombiner
{
    private const bool TO_OBJ = true;
    static void CombineGameObjects(GameObject obj0, GameObject[] objs)
    {
        
        MeshFilter mf0 = obj0.GetComponent<MeshFilter>();
        if (mf0 == null) return;

        List<CombineInstance> combine = new List<CombineInstance>();
        int verticeCount=0;
        foreach (GameObject obj in objs)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf != null)
            {
                Mesh mesh=mf.sharedMesh!=null?mf.sharedMesh:mf.mesh;
                if(mesh==null)continue;
                if (verticeCount + mesh.vertexCount >= 65535)
                {
                    break;
                }
                verticeCount += mesh.vertexCount;
                CombineInstance inst = new CombineInstance
                {
                    mesh = mesh,
                    transform= obj0.transform.worldToLocalMatrix*obj.transform.localToWorldMatrix
                };
                combine.Add(inst);
                if (obj != obj0)
                {
                    if (obj.transform.childCount == 0 && obj.GetComponent<Collider>() == null)
                    {
                        Undo.DestroyObjectImmediate(obj);
                    }else
                    {
                        Undo.DestroyObjectImmediate(mf);
                    }
                }
            }
        }
        if (combine.Count > 1)
        {
            Undo.RecordObject(mf0, "");
            Mesh prevMesh=mf0.sharedMesh;
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine.ToArray(),true,true,true);
            

            string dataPath=Application.dataPath.Replace("\\","/");
            if(dataPath.EndsWith("/Assets"))dataPath=dataPath.Substring(0, dataPath.Length-"/Assets".Length);
            dataPath+="/";
            string path0 = AssetDatabase.GetAssetPath(prevMesh);
            if (path0 == "")
            {
                path0="Assets/MeshCombined";
            }
            else
            {
                if (path0.LastIndexOf('.') > 0) path0 = path0.Substring(0, path0.LastIndexOf('.'));
            }
            if(!File.Exists(dataPath + path0))
            {
                Directory.CreateDirectory(dataPath + path0);
            }
            int i=0;
            string path;
            string name = "";
            do
            {
                name="MeshCombined"+(i!=0?i.ToString():"");
                path=path0+"/"+name+(TO_OBJ?".obj":".asset");
                i++;
            }while(File.Exists(dataPath+path));

            if (!TO_OBJ)
            {
                AssetDatabase.CreateAsset(mesh,path);
                mf0.sharedMesh = mesh;
            }
            else
            {
                string objStr = MeshToObj(mesh,name);
                string absPath = Application.dataPath + path.Substring(6);
                File.WriteAllText(absPath,objStr);
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
                Mesh meshLoaded=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                mf0.sharedMesh = meshLoaded;
            }
            
            EditorUtility.SetDirty(mf0);
        }
    }

    static string MeshToObj(Mesh mesh,string groupName="default")
    {
        StringBuilder str = new StringBuilder();
        str.Append("g "+groupName.Replace(" ","_").Replace("\t","_")+"\n");
        foreach(Vector3 vec in mesh.vertices)
        {
            str.AppendFormat("v {0} {1} {2}\n", vec.x, vec.y, vec.z);
        }
        
        str.Append("\n");
        foreach(Vector2 vec in mesh.uv)
        {
            str.AppendFormat("vt {0} {1}\n", vec.x, vec.y);
        }

        str.Append("\n");
        foreach(Vector3 vec in mesh.normals)
        {
            str.AppendFormat("vn {0} {1} {2}\n", vec.x, vec.y, vec.z);
        }
        
        
        
        str.Append("\n");
        int[] triangles = mesh.triangles;
        int count = triangles.Length;
        bool hasUv = mesh.uv != null && mesh.uv.Length > 0;
        bool hasNormal = mesh.normals != null && mesh.normals.Length > 0;
        
        for(int i=0; i+2 < count;i+=3)
        {
            if (hasUv && hasNormal)
            {
                str.AppendFormat("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", triangles[i]+1, triangles[i+1]+1, triangles[i+2]+1);
            }
            else if (hasUv)
            {
                str.AppendFormat("f {0}/{0} {1}/{1} {2}/{2}\n", triangles[i]+1, triangles[i+1]+1, triangles[i+2]+1);
            }
            else if (hasNormal)
            {
                str.AppendFormat("f {0}//{0} {1}//{1} {2}//{2}\n", triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1);
            }
            else
            {
                str.AppendFormat("f {0} {1} {2}\n", triangles[i]+1, triangles[i+1]+1, triangles[i+2]+1);
            }
            
        }

        return str.ToString();
    }
    
    
    [MenuItem("Tools/合并选中的模型")]
    static void CombineGameObjects()
    {
        GameObject obj0=Selection.activeGameObject;
        GameObject[] objs=Selection.gameObjects;
        if(obj0== null || AssetDatabase.GetAssetPath(obj0) != "" || objs.Length<=1) return;
        MeshFilter mf0=obj0.GetComponent<MeshFilter>();
        if(mf0==null) return;
        CombineGameObjects(obj0,objs);
    }
    
    

    static void getGameObjectsWithMeshes(Transform t,List<GameObject> outGameObjects)
    {
        MeshFilter mf0 = t.GetComponent<MeshFilter>();
        if(mf0!=null && (mf0.sharedMesh!=null || mf0.mesh != null))
        {
            outGameObjects.Add(t.gameObject);
        }
        for(int i = 0; i < t.childCount; i++)
        {
            getGameObjectsWithMeshes(t.GetChild(i), outGameObjects);
        }
    }

    [MenuItem("Tools/合并相同材质的子模型")]
    static void CombineChildGameObjectsWithSameTextures()
    {
        GameObject obj0 = Selection.activeGameObject;
        if (obj0 == null || AssetDatabase.GetAssetPath(obj0) != "") return;
        List<GameObject> objs=new List<GameObject>();
        getGameObjectsWithMeshes(obj0.transform,objs);
        Dictionary<Material,List<GameObject>> dic=new Dictionary<Material, List<GameObject>>();
        foreach(GameObject obj in objs)
        {
            Renderer renderer=obj.GetComponent<Renderer>();
            if(renderer==null || renderer.sharedMaterial == null)continue;
            Material mat= renderer.sharedMaterial;
            if (dic.ContainsKey(mat))
            {
                dic[mat].Add(obj);
            }
            else
            {
                dic.Add(mat,new List<GameObject>{obj});
            }
        }
        foreach(var pair in dic)
        {
            CombineGameObjects(pair.Value[0],pair.Value.ToArray());
        }
    }
    
    [MenuItem("Tools/导出选中的模型到Obj")]
    static void ExportMeshToObj()
    {
        GameObject obj0=Selection.activeGameObject;
        if(obj0==null) return;
        MeshFilter mf0=obj0.GetComponent<MeshFilter>();
        if(mf0==null || mf0.sharedMesh==null) return;
        string path=EditorUtility.SaveFilePanel("保存OBJ模型","",obj0.name+".obj","obj");
        File.WriteAllText(path,MeshToObj(mf0.sharedMesh,obj0.name));
        AssetDatabase.Refresh();
    }
}
