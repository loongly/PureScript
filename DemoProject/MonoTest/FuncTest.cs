using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FuncTest
{
/*
    static void Main()
    {
        Console.WriteLine("@@@@ ==== " );
        StartTest();

    }*/
    public static void Main(string[] args)
    {
        StartTest();
    }

    /*public static void TestMain(IntPtr ptr)
    {
       
    }*/


    // Start is called before the first frame update
    static void StartTest()
    {
        Debug.LogError("223");
        Debug.LogError(" ======== 223  DDD ==== ");
        Debug.LogError(" ======== 中午 付哈 ==== +-*x&!@$#$()_+<>?{}|ff ~");

        GameObject.CreatePrimitive(PrimitiveType.Sphere);

        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

        var t = obj.transform;
        t.position = new Vector3(3, 2, 1);

        var t2 = obj.transform;
        if(t == t2)
        {
            Debug.LogError("eql transform");
            t2.position = new Vector3(-3, 2, 1);
        }
        var pos = t2.position;

        Debug.LogError(obj.name);
        Debug.LogError("==== "+obj.name+"!@$%%%^^*(& === ");
        Debug.LogError($"== x:{pos.x} y:{pos.y} z:{pos.z}");

        obj.AddComponent<TestBehaviourScript>();

        return;
        TestStaticClass.StartTest(1);
        
        
    }

}
