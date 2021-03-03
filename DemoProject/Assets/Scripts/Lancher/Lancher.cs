using PureScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class Lancher : MonoBehaviour
{
    string reloadDir;
    string loadUrl;
    string infoStr;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_STANDALONE_WIN
        reloadDir = @"F:\Project\UnityBind\PureScript\ScriptEngine\Managed";
#else
        reloadDir =  Path.Combine(Application.persistentDataPath , "Managed");
#endif
        loadUrl = PlayerPrefs.GetString("reload_url", "http://192.168.50.168:8090/Code.dll");
    }

    void OnGUI()
    {
        loadUrl = GUI.TextField(new Rect(20, 20, 300, 40), loadUrl);
        if (GUI.Button(new Rect(330, 20, 80, 40), "Reload"))
        {
            PlayerPrefs.SetString("reload_url", loadUrl);
            PlayerPrefs.Save();
            StartCoroutine(ReloadDll(loadUrl));
        }

        if (GUI.Button(new Rect(20,80,120,70),"Start"))
        {
#if UNITY_EDITOR
            //MonoEntry.Main();
#else
            ScriptEngine.Setup(reloadDir, "TestEntry.dll");

            /* equal to:
            Assembly assembly = Assembly.Load("TestEntry.dll");
            Type type = assembly.GetType("MonoEntry");
            MethodInfo mi = type.GetMethod("Main");
            var res = mi.Invoke(null, null);
        */
#endif
        }

        if (!string.IsNullOrEmpty(infoStr))
            GUI.Label(new Rect(20, 170, 300, 60), infoStr);
    }

    IEnumerator ReloadDll(string url)
    {
        using (UnityWebRequest downloader = UnityWebRequest.Get(url))
        {
            var fileName = Path.GetFileName(url);
            downloader.downloadHandler = new DownloadHandlerFile(reloadDir + "/" + fileName);

            infoStr = "downloading..";
            yield return  downloader.SendWebRequest();

            if (downloader.isHttpError || downloader.isNetworkError || downloader.error != null)
            {
                infoStr = "error : "+ downloader.error;
            }
            else
            {
                infoStr = "download success.";
                yield return new WaitForSeconds(3.0f);
                infoStr = null;
            }
        }

    }
}
