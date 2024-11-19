using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 注意： Stream操作的路径不需要加"file://"
/// UnityWebRequest操作的路径根据平台需要加"file://"
/// </summary>
public class Test : MonoBehaviour
{
    private string GetPathFormPersistent(string relativePath)
    {
        string pre = "";
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        pre = "";
#elif UNITY_ANDROID
        pre = "";
#elif UNITY_IPHONE
        pre = "";
#endif
        string path = pre + Application.persistentDataPath + $"/{relativePath}";
        return path;
    }
    private string GetPathFormStreaming(string relativePath)
    {
        string pre = "file://";
#if UNITY_EDITOR
        pre = "file://";
#elif UNITY_ANDROID
        pre = "";
#elif UNITY_IPHONE
	    pre = "file://";
#endif
        string path = pre + Application.streamingAssetsPath + $"/{relativePath}";
        return path;
    }
    string sourceName = "AtomicEntitycsv.csv";
    string targetName = "AtomicEntitycsv.csv";
    string sourcePath;
    string targetPath;
    private void Start()
    {
        sourcePath = GetPathFormStreaming(sourceName);
        targetPath = GetPathFormPersistent(targetName);
        Debug.Log($"sourcePath：{sourcePath},targetPath:{targetPath}");
    }
    private void Print(List<string> data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            Debug.Log($"[Print]data{i} = {data[i]}");
        }
    }
    // Start is called before the first frame update
    public void OnButton1Click()
    {
        Debug.Log($"[TestIOS]OnButton1Click>>从Streaming中读取:{sourcePath}");
        string content = WWWReadFile(sourcePath);
        Debug.Log($"content:{content}");
    }
    public void OnButton2Click()
    {
        Debug.Log($"[TestIOS]OnButton2Click>>从{sourcePath}中复制文件到{targetPath}");
        string content = WWWReadFile(sourcePath);
        File.WriteAllText(targetPath, content, Encoding.GetEncoding("utf-8"));
    }
    public void OnButton3Click()
    {
        Debug.Log("[TestIOS]OnButton3Click>>>>从Persistent中读取");
        StreamReadFile(targetPath, (List<string> data) => {
            Print(data);
        });
    }

    public void OnButton4Click()
    {
        Debug.Log($"[TestIOS]OnButton4Click>>复制文件{GetPathFormStreaming(sourceName)}");
        StartCoroutine(copy());
    }

    /// <summary>
    /// 将streaming path 下的文件copy到对应用
    /// 为什么不直接用io函数拷贝，原因在于streaming目录不支持，
    /// 不管理是用getStreamingPath_for_www，还是Application.streamingAssetsPath，
    /// io方法都会说文件不存在
    /// </summary>
    /// <param name="fileName"></param>
    private IEnumerator copy()
    {
        string src = sourcePath;
        string des = targetPath;// Application.persistentDataPath + "/" + fileName;
        Debug.Log("des:" + des);
        Debug.Log("src:" + src);
        UnityWebRequest request = UnityWebRequest.Get(src);
        request.SendWebRequest();//读取数据
        if (request.error == null)
        {
            while (true)
            {
                if (request.downloadHandler.isDone)//是否读取完数据
                {
                    Debug.Log("开始复制文件：" + request.downloadHandler.text);
                    if (File.Exists(des))
                    {
                        File.Delete(des);
                    }
                    FileStream fsDes = File.Create(des);
                    fsDes.Write(request.downloadHandler.data, 0, request.downloadHandler.data.Length);
                    fsDes.Flush();
                    fsDes.Close();
                    Debug.Log("复制文件结束");
                    yield break;
                }
            }
        }
        else
        {
            Debug.LogError("UnityWebRequest is error!");
            yield break;
        }
    }

    /// <summary>
    /// 从文件流中读取
    /// </summary>
    /// <param name="path"></param>
    /// <param name="ac"></param>
    private void StreamReadFile(string path, Action<List<string>> ac)
    {
        StreamReader sr;
        List<string> data = new List<string>();
        try
        {
            using (sr = new StreamReader(path, Encoding.UTF8))
            {
                string str = "";
                while ((str = sr.ReadLine()) != null)
                {
                    data.Add(str);
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("StreamReader 文件读取发生错误，" + ex);
            
        }
        ac?.Invoke(data);
    }

    /// <summary>
    /// UnityWebRequest读取文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private string WWWReadFile(string path)
    {
        UnityWebRequest request = UnityWebRequest.Get(path);
        request.SendWebRequest();//读取数据
        while (true)
        {
            if (request.downloadHandler.isDone)//是否读取完数据
            {
                return request.downloadHandler.text;
            }
        }
    }
}
