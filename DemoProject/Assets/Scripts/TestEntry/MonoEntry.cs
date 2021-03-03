using PureScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

/// <summary>
/// by default, mono runtime call specific classes(MonoEntry) and specific method(Main) on setup.
/// </summary>
public class MonoEntry
{
    public static void Main()
    {
        TestCube();

        TestLoadAssembly();
    }

    static void TestLoadAssembly()
    {
        var dllPath = Application.persistentDataPath + $"/Managed/{StartInfo.ReloadDllName}";
        if(File.Exists(dllPath))
        {
            Assembly assembly = Assembly.LoadFrom(dllPath);
            Type type = assembly.GetType(StartInfo.ReloadClassName);
            MethodInfo mi = type.GetMethod(StartInfo.TestMethodName);
            var res = mi.Invoke(null, new object[] {"hello"});
            Debug.LogError("StartTest res: " + res);
        }
        
    }


    static void TestCube()
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

        var t = obj.transform;
        t.position = new Vector3(3, 2, 1);

        var t2 = obj.transform;
        if (t == t2)
        {
            Debug.LogError("eql transform");
            t2.position = new Vector3(-3, 2, 1);
        }
        var pos = t2.position;

        Debug.LogError("==== " + obj.name + "!@$%%%^^*(& === ");
        Debug.LogError($"== x:{pos.x} y:{pos.y} z:{pos.z}");
    }

}
