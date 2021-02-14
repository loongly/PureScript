using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClass 
{
    private int argA;
    private string argStr;
    private TestBehaviourScript script;

    public int StartTest(int a)
    {
        GameObject testObj = new GameObject("testObj");
        script = testObj.AddComponent<TestBehaviourScript>();
        argStr = testObj.name;
        Debug.LogError(argStr);
        argA = 1 + a;
        return argA;
    }
}
