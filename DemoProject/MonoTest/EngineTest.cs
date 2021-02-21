using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using PureScript;

public class EngineTest
{

    public static void Main(string[] args)
    {

        try
        {
            StartTest();
        }catch(Exception e)
        {
            Debug.LogError(e.ToString());
        }
        
    }

    static void StartTest()
    {
        //var ptr = ScriptEngine.GetFuncPointer();
        //MonoBind.InitBind(ptr);

        //DebugHelper.InitLog(true);

        Debug.LogError(" ========223 中午 ==== +-*x&!@$#$()_+<>?{}|ff ~");

        TestStaticClass.StartTest(1);

        TestCube();

        var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //obj.AddComponent<TestDelegate>();
        obj.AddComponent<TestLoader>();
        obj.AddComponent<TestBehaviourScript>();
        //obj.AddComponent<TestException>();
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
