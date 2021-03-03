using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClass 
{
    private int argA;
    private string argStr;

    public int StartTest(int a)
    {
        GameObject testObj = new GameObject("testObj");
        argStr = testObj.name;
        Debug.LogError(argStr);
        argA = 1 + a;
        testObj = null;
        return argA;
    }

    ~TestClass()
    {
        Debug.LogError("~TestClass");
    }
}
