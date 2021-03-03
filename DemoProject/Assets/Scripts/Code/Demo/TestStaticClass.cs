using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class TestStaticClass 
{
    static TestClass testClass;
    public static void StartTest(int s)
    {

        Debug.LogError("==== persistentDataPath: " + Application.streamingAssetsPath);
        Debug.LogError("==== isPlaying: " + Application.isPlaying);
        Debug.LogError("==== platform: " + Application.platform);
        Application.targetFrameRate = 60;
       // Application.OpenURL("http://www.baidu.com");


        testClass = new TestClass();
        testClass.StartTest(s + 1);
    }

    
}
