using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadTest 
{
    public static int StartTest(string info)
    {
        DebugHelper.InitLog(true);

        Debug.LogError(" == 223 中午 == +-*x&!@$#$()_+<>?{}|ff ~ " + info);

        TestStaticClass.StartTest(1);

        var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.AddComponent<TestDelegate>();
        obj.AddComponent<TestLoader>();
        obj.AddComponent<TestBehaviourScript>();
        //obj.AddComponent<TestException>();
        obj.AddComponent<TestUGUI>();

        return 123;
    }


}
