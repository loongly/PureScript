using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EngineTest
{

    public static void Main(string[] args)
    {
        StartTest();
    }

    static void StartTest()
    {
        DebugHelper.InitLog(true);

        Debug.LogError(" ========223 中午 ==== +-*x&!@$#$()_+<>?{}|ff ~");

        TestStaticClass.StartTest(1);

        TestCube();

        var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.AddComponent<TestDelegate>();
        obj.AddComponent<TestLoader>();
        obj.AddComponent<TestBehaviourScript>();
        
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
